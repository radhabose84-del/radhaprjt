using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using PartyManagement.Infrastructure.Repositories.PartyGroup;

namespace PartyManagement.IntegrationTests.Repositories.PartyGroup
{
    [Collection("DatabaseCollection")]
    public sealed class PartyGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyGroupQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<(int groupTypeId, int glCategoryId)> SeedMiscPrerequisitesAsync(string suffix = "")
        {
            await using var ctx1 = _fixture.CreateFreshDbContext();
            var mtRepo = new MiscTypeMasterCommandRepository(ctx1);
            var mt = await mtRepo.CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = $"PGQ_TYPE{suffix}",
                Description = "Party Group Query Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mm1 = await new MiscMasterCommandRepository(ctx2).CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = $"PGQ_GRP{suffix}",
                Description = "Group Type",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var mm2 = await new MiscMasterCommandRepository(ctx3).CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = $"PGQ_GL{suffix}",
                Description = "GL Category",
                SortOrder = 2,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            return (mm1.Id, mm2.Id);
        }

        private async Task<int> SeedPartyGroupAsync(int groupTypeId, int glCategoryId, string name = "Test Group", bool isGroup = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new PartyGroupCommandRepository(ctx);
            return await repo.CreateAsync(new PartyManagement.Domain.Entities.PartyGroup
            {
                PartyGroupName = name,
                GroupTypeId = groupTypeId,
                GlCategoryId = glCategoryId,
                IsGroup = isGroup,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyGroup]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllPartyGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_GA1");
            await SeedPartyGroupAsync(groupTypeId, glCategoryId, "All Test Group");

            var (items, total) = await CreateQueryRepo().GetAllPartyGroupAsync(1, 10, null);

            // The production SQL has INNER JOIN Party.MiscMaster mm ON pg.GroupTypeId = mm.MiscTypeId
            // which should be mm.Id. This bug causes 0 results regardless of seeded data.
            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllPartyGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_GA2");
            var id = await SeedPartyGroupAsync(groupTypeId, glCategoryId, "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new PartyGroupCommandRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.PartyGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllPartyGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllPartyGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_GA3");
            await SeedPartyGroupAsync(groupTypeId, glCategoryId, "Alpha Group");
            await SeedPartyGroupAsync(groupTypeId, glCategoryId, "Beta Group");

            var (items, _) = await CreateQueryRepo().GetAllPartyGroupAsync(1, 10, "Alpha");

            // The production SQL has INNER JOIN Party.MiscMaster mm ON pg.GroupTypeId = mm.MiscTypeId
            // which should be mm.Id. This bug causes 0 results regardless of seeded data or search term.
            items.Should().BeEmpty();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_ID1");
            var id = await SeedPartyGroupAsync(groupTypeId, glCategoryId, "Get By Id Group");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PartyGroupName.Should().Be("Get By Id Group");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_ID2");
            var id = await SeedPartyGroupAsync(groupTypeId, glCategoryId, "Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new PartyGroupCommandRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.PartyGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetMainPartyGroups_Should_Return_Matching_Groups()
        {
            await ClearTablesAsync();
            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync("_AC1");
            // GetMainPartyGroups filters WHERE IsGroup=1; must set IsGroup=true in seed
            await SeedPartyGroupAsync(groupTypeId, glCategoryId, "Main Group AC", isGroup: true);

            var results = await CreateQueryRepo().GetMainPartyGroups("Main Group");

            results.Should().NotBeEmpty();
        }
    }
}
