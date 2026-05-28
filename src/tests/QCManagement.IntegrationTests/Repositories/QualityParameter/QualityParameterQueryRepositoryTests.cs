using Microsoft.Data.SqlClient;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QualityParameter
{
    [Collection("DatabaseCollection")]
    public sealed class QualityParameterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityParameterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualityParameterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new QualityParameterQueryRepository(conn);
        }

        private QualityParameterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new QualityParameterCommandRepository(ctx);

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync("QC.QualitySpecificationParameter", "QC.QualitySpecification", "QC.QualityTemplateParameter", "QC.QualityTemplate", "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<(int groupId, int dataTypeId, int validationTypeId, int otherDataTypeId)> SeedDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var groupType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_GROUP", Description = "Group", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_DATATYPE", Description = "Data Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_VALIDATION", Description = "Validation Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var groupId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "MEC", Description = "Mechanical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "DEC", Description = "Decimal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var otherDataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "TXT", Description = "Text", SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            return (groupId, dataTypeId, validationTypeId, otherDataTypeId);
        }

        private Domain.Entities.QualityParameter BuildEntity(int groupId, int dataTypeId, int validationTypeId, string code = "QP-000001", string name = "Yarn Tensile", int? unitId = null, bool isActive = true) =>
            new Domain.Entities.QualityParameter
            {
                ParameterCode = code,
                ParameterName = name,
                ParameterGroupId = groupId,
                DataTypeId = dataTypeId,
                UnitId = unitId,
                ValidationTypeId = validationTypeId,
                Description = "Test description",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedAsync(Domain.Entities.QualityParameter entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Records()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "P1"));
            await SeedAsync(BuildEntity(g, d, v, "QP-000002", "P2"));

            var (data, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_GroupCode_FromJoin()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v));

            var (data, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data[0].ParameterGroupCode.Should().Be("MEC");
            data[0].DataTypeCode.Should().Be("DEC");
            data[0].ValidationTypeCode.Should().Be("RNG");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_ParameterGroupId()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v));

            var (data, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, g);

            total.Should().Be(1);
            data[0].ParameterGroupId.Should().Be(g);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Tensile"));
            await SeedAsync(BuildEntity(g, d, v, "QP-000002", "Moisture"));

            var (data, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Tensile");

            total.Should().Be(1);
            data[0].ParameterName.Should().Be("Tensile");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_WithJoinedFields()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            var id = await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Tensile"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ParameterCode.Should().Be("QP-000001");
            dto.ParameterGroupCode.Should().Be("MEC");
            dto.DataTypeCode.Should().Be("DEC");
            dto.ValidationTypeCode.Should().Be("RNG");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearAllAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenNameExists()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Tensile Strength"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("Tensile Strength");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Be_CaseInsensitive()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Tensile Strength"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("tensile strength");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludingSelf()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            var id = await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Tensile"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("Tensile", id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenExists()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            var id = await SeedAsync(BuildEntity(g, d, v));

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenNotExists()
        {
            await ClearAllAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ParameterGroupExistsAsync_Should_Return_True_ForValidGroup()
        {
            await ClearAllAsync();
            var (g, _, _, _) = await SeedDependenciesAsync();

            var result = await CreateQueryRepo().ParameterGroupExistsAsync(g);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ParameterGroupExistsAsync_Should_Return_False_ForWrongType()
        {
            await ClearAllAsync();
            var (_, d, _, _) = await SeedDependenciesAsync();

            // d is a DataType row, not a Group row — should return false
            var result = await CreateQueryRepo().ParameterGroupExistsAsync(d);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DataTypeExistsAsync_Should_Return_True_ForValidDataType()
        {
            await ClearAllAsync();
            var (_, d, _, _) = await SeedDependenciesAsync();

            var result = await CreateQueryRepo().DataTypeExistsAsync(d);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidationTypeExistsAsync_Should_Return_True_ForValidValidationType()
        {
            await ClearAllAsync();
            var (_, _, v, _) = await SeedDependenciesAsync();

            var result = await CreateQueryRepo().ValidationTypeExistsAsync(v);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUomRequiredForDataTypeAsync_Should_Return_True_ForDecimal()
        {
            await ClearAllAsync();
            var (_, d, _, _) = await SeedDependenciesAsync();

            // d is DEC — should require UOM
            var result = await CreateQueryRepo().IsUomRequiredForDataTypeAsync(d);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUomRequiredForDataTypeAsync_Should_Return_False_ForText()
        {
            await ClearAllAsync();
            var (_, _, _, txt) = await SeedDependenciesAsync();

            // txt is TXT — UOM not required
            var result = await CreateQueryRepo().IsUomRequiredForDataTypeAsync(txt);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetDataTypeIdByQualityParameterIdAsync_Should_Return_DataTypeId()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            var id = await SeedAsync(BuildEntity(g, d, v));

            var result = await CreateQueryRepo().GetDataTypeIdByQualityParameterIdAsync(id);

            result.Should().Be(d);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_ByCodeOrName()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Yarn Tensile"));
            await SeedAsync(BuildEntity(g, d, v, "QP-000002", "Moisture"));

            var results = await CreateQueryRepo().AutocompleteAsync("Tensile", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ParameterName.Should().Be("Yarn Tensile");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive()
        {
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            await SeedAsync(BuildEntity(g, d, v, "QP-000001", "Active P", isActive: true));
            await SeedAsync(BuildEntity(g, d, v, "QP-000002", "Inactive P", isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("P", CancellationToken.None);

            results.Should().Contain(r => r.ParameterName == "Active P");
            results.Should().NotContain(r => r.ParameterName == "Inactive P");
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_ForNoDependents()
        {
            // No entities depend on QualityParameter yet (Quality Template / Spec / Inspection are future).
            await ClearAllAsync();
            var (g, d, v, _) = await SeedDependenciesAsync();
            var id = await SeedAsync(BuildEntity(g, d, v));

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
