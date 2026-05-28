using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.Infrastructure.Repositories.QualityTemplate;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QualityTemplate
{
    [Collection("DatabaseCollection")]
    public sealed class QualityTemplateQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityTemplateQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualityTemplateQueryRepository CreateQueryRepo(Mock<IUOMLookup> uomLookup = null)
        {
            var lookup = uomLookup ?? BuildDefaultUomLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new QualityTemplateQueryRepository(conn, lookup.Object);
        }

        private Mock<IUOMLookup> BuildDefaultUomLookup(int uomId = 1, string code = "KG", string name = "Kilogram")
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new UOMLookupDto { Id = uomId, Code = code, UOMName = name } });
            return mock;
        }

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync(
                "QC.QualitySpecificationParameter", "QC.QualitySpecification",
                "QC.QualityTemplateParameter", "QC.QualityTemplate",
                "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<(int qpId, int inspMethodId)> SeedDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var groupType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_GROUP", Description = "Group", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_DATATYPE", Description = "Data Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_VALIDATION", Description = "Validation Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var inspectionMethodType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_INSPECTION_METHOD", Description = "Inspection Method", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var groupId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "MEC", Description = "Mechanical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "DEC", Description = "Decimal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var inspMethodId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = inspectionMethodType, Code = "LAB", Description = "Lab Test", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var qpId = await new QualityParameterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityParameter
                {
                    ParameterCode = "QP-000001",
                    ParameterName = "Tensile",
                    ParameterGroupId = groupId,
                    DataTypeId = dataTypeId,
                    ValidationTypeId = validationTypeId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            return (qpId, inspMethodId);
        }

        private async Task<int> SeedTemplateAsync(int qpId, int inspMethodId, string code = "QT-000001", string name = "Yarn QC", bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new QualityTemplateCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityTemplate
                {
                    TemplateCode = code,
                    TemplateName = name,
                    Description = "Standard QC checks",
                    IsActive = isActive ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted,
                    QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>
                    {
                        new Domain.Entities.QualityTemplateParameter
                        {
                            QualityParameterId = qpId,
                            SequenceNo = 1,
                            IsMandatory = true,
                            IsCritical = false,
                            InspectionMethodId = inspMethodId,
                            SampleSize = 5,
                            SampleUomId = 1,
                            IsGradeApplicable = true,
                            Remarks = "Test",
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }
                    }
                });
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Records()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            await SeedTemplateAsync(qpId, inspId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].ParameterCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            await SeedTemplateAsync(qpId, inspId, "QT-000001", "Yarn QC");
            await SeedTemplateAsync(qpId, inspId, "QT-000002", "Fabric QC");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Yarn");

            items.Should().HaveCount(1);
            items[0].TemplateName.Should().Be("Yarn QC");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            var id = await SeedTemplateAsync(qpId, inspId);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new QualityTemplateCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            }

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_And_Details()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            var id = await SeedTemplateAsync(qpId, inspId);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.TemplateName.Should().Be("Yarn QC");
            dto.Parameters.Should().HaveCount(1);
            dto.Parameters![0].QualityParameterId.Should().Be(qpId);
            dto.Parameters![0].InspectionMethodId.Should().Be(inspId);
            dto.Parameters![0].ParameterName.Should().Be("Tensile");
            dto.Parameters![0].InspectionMethodName.Should().Be("Lab Test");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_UomFromLookup()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            var id = await SeedTemplateAsync(qpId, inspId);

            var customLookup = BuildDefaultUomLookup(uomId: 1, code: "MTR", name: "Meter");
            var dto = await CreateQueryRepo(customLookup).GetByIdAsync(id);

            dto!.Parameters![0].SampleUomCode.Should().Be("MTR");
            dto.Parameters![0].SampleUomName.Should().Be("Meter");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAllAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            await SeedTemplateAsync(qpId, inspId, name: "Duplicate Name");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Duplicate Name");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            var id = await SeedTemplateAsync(qpId, inspId, name: "Dup");

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new QualityTemplateCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            }

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Dup");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearAllAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            await SeedTemplateAsync(qpId, inspId, name: "Inactive QC", isActive: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task QualityParameterExistsAndActiveAsync_Should_Return_True_For_Active()
        {
            await ClearAllAsync();
            var (qpId, _) = await SeedDependenciesAsync();

            var exists = await CreateQueryRepo().QualityParameterExistsAndActiveAsync(qpId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task InspectionMethodExistsAsync_Should_Return_True_For_Active()
        {
            await ClearAllAsync();
            var (_, inspMethodId) = await SeedDependenciesAsync();

            var exists = await CreateQueryRepo().InspectionMethodExistsAsync(inspMethodId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetMaxTemplateCodeSequenceAsync_Should_Return_Zero_When_Empty()
        {
            await ClearAllAsync();

            var max = await CreateQueryRepo().GetMaxTemplateCodeSequenceAsync();

            max.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxTemplateCodeSequenceAsync_Should_Return_Highest_Sequence()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();
            await SeedTemplateAsync(qpId, inspId, code: "QT-000001", name: "A");
            await SeedTemplateAsync(qpId, inspId, code: "QT-000005", name: "B");
            await SeedTemplateAsync(qpId, inspId, code: "QT-000003", name: "C");

            var max = await CreateQueryRepo().GetMaxTemplateCodeSequenceAsync();

            max.Should().Be(5);
        }
    }
}
