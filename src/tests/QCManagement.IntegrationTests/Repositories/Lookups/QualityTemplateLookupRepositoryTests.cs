using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using QCManagement.Infrastructure.Repositories.Lookups;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.Infrastructure.Repositories.QualityTemplate;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class QualityTemplateLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityTemplateLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualityTemplateLookupRepository CreateRepo(Mock<IUOMLookup>? uomLookup = null)
        {
            var lookup = uomLookup ?? BuildDefaultUomLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new QualityTemplateLookupRepository(conn, lookup.Object);
        }

        private Mock<IUOMLookup> BuildDefaultUomLookup(int uomId = 1, string code = "KG", string name = "Kilogram")
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = uomId, Code = code, UOMName = name } });
            return mock;
        }

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync(
                "QC.QualitySpecificationParameter", "QC.QualitySpecification",
                "QC.QualityTemplateParameter", "QC.QualityTemplate",
                "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<int> SeedQualityParameterAsync(string code = "QP-000001", string name = "Staple", bool isActive = true)
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
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "PHY", Description = "Physical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "TXT", Description = "Text", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            return await new QualityParameterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityParameter
                {
                    ParameterCode = code,
                    ParameterName = name,
                    ParameterGroupId = groupId,
                    DataTypeId = dataTypeId,
                    ValidationTypeId = validationTypeId,
                    UnitId = 1,
                    IsActive = isActive ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedTemplateAsync(int qpId, string code = "QT-000001", string name = "Cotton Passing", bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new QualityTemplateCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityTemplate
                {
                    TemplateCode = code,
                    TemplateName = name,
                    Description = "Cotton quality template",
                    IsActive = isActive ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted,
                    QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>
                    {
                        new()
                        {
                            QualityParameterId = qpId,
                            SequenceNo = 1,
                            IsMandatory = true,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }
                    }
                });
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Template()
        {
            await ClearAllAsync();
            var qpId = await SeedQualityParameterAsync();
            var templateId = await SeedTemplateAsync(qpId);

            var result = await CreateRepo().GetByIdsAsync(new[] { templateId });

            result.Should().HaveCount(1);
            result[0].TemplateName.Should().Be("Cotton Passing");
        }

        [Fact]
        public async Task GetByIdsAsync_EmptyInput_Should_Return_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetParametersByTemplateIdAsync_Should_Return_Active_Params_With_UnitName()
        {
            await ClearAllAsync();
            var qpId = await SeedQualityParameterAsync(name: "Micronaire");
            var templateId = await SeedTemplateAsync(qpId);

            var result = await CreateRepo().GetParametersByTemplateIdAsync(templateId);

            result.Should().HaveCount(1);
            result[0].QualityParameterId.Should().Be(qpId);
            result[0].ParameterName.Should().Be("Micronaire");
            result[0].UnitName.Should().Be("Kilogram");
        }

        [Fact]
        public async Task GetParametersByTemplateIdAsync_Should_Exclude_Inactive_Parameter()
        {
            await ClearAllAsync();
            var qpId = await SeedQualityParameterAsync(isActive: false);
            var templateId = await SeedTemplateAsync(qpId);

            var result = await CreateRepo().GetParametersByTemplateIdAsync(templateId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetParametersByIdsAsync_Should_Return_Parameter()
        {
            await ClearAllAsync();
            var qpId = await SeedQualityParameterAsync(name: "Trash");

            var result = await CreateRepo().GetParametersByIdsAsync(new[] { qpId });

            result.Should().HaveCount(1);
            result[0].ParameterName.Should().Be("Trash");
        }
    }
}
