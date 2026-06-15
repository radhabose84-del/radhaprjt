using Microsoft.Data.SqlClient;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private MiscMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new MiscMasterCommandRepository(ctx);

        private async Task<int> SeedParentMiscTypeAsync(string code = "QP_GROUP", string description = "Quality Parameter Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscTypeMasterCommandRepository(ctx)
                .CreateAsync(new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "PHY",
            string description = "Physical",
            int sortOrder = 1,
            bool isActive = true) =>
            new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedEntityAsync(Domain.Entities.MiscMaster entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearTablesAsync("QC.QcInspectionDtl", "QC.QcInspectionHdr", "QC.QualitySpecificationParameter", "QC.QualitySpecification", "QC.QualityTemplateParameter", "QC.QualityTemplate", "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            await SeedEntityAsync(BuildEntity(parentId, "PHY", "Physical", 1));
            await SeedEntityAsync(BuildEntity(parentId, "CHM", "Chemical", 2));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_MiscTypeCode_FromJoin()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync(code: "QP_DATATYPE", description: "Data Type");
            await SeedEntityAsync(BuildEntity(parentId, "NUM", "Numeric", 1));

            var (data, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().HaveCount(1);
            data[0].MiscTypeCode.Should().Be("QP_DATATYPE");
            data[0].MiscTypeDescription.Should().Be("Data Type");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearTablesAsync();
            var typeA = await SeedParentMiscTypeAsync(code: "QP_GROUP");
            var typeB = await SeedParentMiscTypeAsync(code: "QP_DATATYPE", description: "Data Type");
            await SeedEntityAsync(BuildEntity(typeA, "PHY", "Physical", 1));
            await SeedEntityAsync(BuildEntity(typeB, "NUM", "Numeric", 1));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null, typeA);

            totalCount.Should().Be(1);
            data[0].Code.Should().Be("PHY");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            await SeedEntityAsync(BuildEntity(parentId, "PHY", "Physical", 1));
            await SeedEntityAsync(BuildEntity(parentId, "CHM", "Chemical", 2));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "Phy");

            totalCount.Should().Be(1);
            data[0].Code.Should().Be("PHY");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId, "DEL", "Deleted", 1));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.Code == "DEL");
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO_WithJoinedFields()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync(code: "QP_GROUP", description: "Group");
            var id = await SeedEntityAsync(BuildEntity(parentId, "PHY", "Physical", 1));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.Code.Should().Be("PHY");
            dto.Description.Should().Be("Physical");
            dto.MiscTypeCode.Should().Be("QP_GROUP");
            dto.MiscTypeDescription.Should().Be("Group");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTablesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId, "SDEL", "Deleted", 1));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExistsInSameType()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            await SeedEntityAsync(BuildEntity(parentId, "PHY", "Physical", 1));

            var result = await CreateQueryRepo().AlreadyExistsAsync("PHY", parentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeExistsInDifferentType()
        {
            await ClearTablesAsync();
            var typeA = await SeedParentMiscTypeAsync(code: "QP_GROUP");
            var typeB = await SeedParentMiscTypeAsync(code: "QP_DATATYPE", description: "Data Type");
            await SeedEntityAsync(BuildEntity(typeA, "PFL", "Pass/Fail Group", 1));

            // PFL exists in typeA but not in typeB — composite uniqueness on (MiscTypeId, Code)
            var result = await CreateQueryRepo().AlreadyExistsAsync("PFL", typeB);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludingSelf()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId, "EXCL", "Self", 1));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXCL", parentId, id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId));

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_WhenActiveTypeExists()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            var result = await CreateQueryRepo().MiscTypeExistsAsync(parentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_WhenTypeNotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().MiscTypeExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            await SeedEntityAsync(BuildEntity(parentId, "PHY", "Physical", 1));
            await SeedEntityAsync(BuildEntity(parentId, "PER", "Performance", 2));
            await SeedEntityAsync(BuildEntity(parentId, "VIS", "Visual", 3));

            var results = await CreateQueryRepo().AutocompleteAsync("P", null, CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.Code).Should().Contain(new[] { "PHY", "PER" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_MiscTypeCode()
        {
            await ClearTablesAsync();
            var typeA = await SeedParentMiscTypeAsync(code: "QP_GROUP");
            var typeB = await SeedParentMiscTypeAsync(code: "QP_DATATYPE", description: "Data Type");
            await SeedEntityAsync(BuildEntity(typeA, "PHY", "Physical", 1));
            await SeedEntityAsync(BuildEntity(typeB, "NUM", "Numeric", 1));

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, "QP_GROUP", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Code.Should().Be("PHY");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            await SeedEntityAsync(BuildEntity(parentId, "ACT", "Active", 1, isActive: true));
            await SeedEntityAsync(BuildEntity(parentId, "INA", "Inactive", 2, isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, null, CancellationToken.None);

            results.Should().NotContain(r => r.Code == "INA");
            results.Should().Contain(r => r.Code == "ACT");
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_WhenNoDependents()
        {
            // QualityParameter not yet implemented — repo stub returns false unconditionally.
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId));

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsMiscMasterLinkedAsync_Should_Return_False_WhenNoDependents()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();
            var id = await SeedEntityAsync(BuildEntity(parentId));

            var result = await CreateQueryRepo().IsMiscMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
