using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using PartyManagement.Infrastructure.Repositories.PartyMaster;

namespace PartyManagement.IntegrationTests.Repositories.PartyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        /// <summary>
        /// Seeds the required ApprovalStatus MiscTypeMaster and Pending MiscMaster entries.
        /// CreateAsync looks up MiscTypeCode="ApprovalStatus", Code="Pending" to set StatusId.
        /// </summary>
        private async Task SeedApprovalStatusAsync()
        {
            await using var ctx1 = _fixture.CreateFreshDbContext();
            var mt = await new MiscTypeMasterCommandRepository(ctx1).CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus",
                Description = "Approval Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx2).CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "Pending",
                Description = "Pending",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static PartyManagement.Domain.Entities.PartyMaster BuildEntity(
            string code = "P0001",
            string name = "Test Party") =>
            new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = code,
                PartyName = name,
                UnitId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            // Delete child tables in FK-safe order before PartyMaster
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyActivityLog]");
            await conn.ExecuteAsync("DELETE FROM [Party].[AgentConfig]");
            await conn.ExecuteAsync("DELETE FROM [Party].[SalesType]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyUnitCompanyMapping]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyDocument]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyBank]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyAddress]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyContact]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyType]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyGroup]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankAccount]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_PartyName()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Persisted Party"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.PartyName.Should().Be("Persisted Party");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_From_PendingMiscMaster()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Status Test Party"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyMaster.FirstOrDefaultAsync(x => x.Id == id);

            // CreateAsync sets StatusId from MiscMaster where MiscTypeCode=ApprovalStatus, Code=Pending
            saved!.StatusId.Should().BeGreaterThan(0);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Found()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Delete Test Party"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.PartyMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Soft Delete Party"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.PartyMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.PartyMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new PartyManagement.Domain.Entities.PartyMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }

        // --- EXISTS ---

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_PartyName_Exists()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Existing Party"));
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepository(ctx).ExistsAsync("Existing Party");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_PartyName_NotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var exists = await CreateRepository(ctx).ExistsAsync("NonExistent Party");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_IsCaseInsensitive()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Case Party"));
            ctx.ChangeTracker.Clear();

            // ExistsAsync uses Collate CI_AS (case-insensitive)
            var exists = await CreateRepository(ctx).ExistsAsync("case party");

            exists.Should().BeTrue();
        }

        // --- EXISTS FOR UPDATE ---

        [Fact]
        public async Task ExistsForUpdateAsync_Should_Return_False_When_Same_Id()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("P0001", "Update Check Party"));
            ctx.ChangeTracker.Clear();

            // Checking its own name — should return false (not a duplicate for update)
            var exists = await CreateRepository(ctx).ExistsForUpdateAsync("Update Check Party", id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsForUpdateAsync_Should_Return_True_When_Another_Party_Has_Same_Name()
        {
            await ClearTablesAsync();
            await SeedApprovalStatusAsync();

            await using var ctx1 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx1).CreateAsync(BuildEntity("P0001", "Shared Name Party"));

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var id2 = await CreateRepository(ctx2).CreateAsync(BuildEntity("P0002", "My Party"));
            ctx2.ChangeTracker.Clear();

            // Trying to rename P0002 to the name that P0001 already has
            var exists = await CreateRepository(ctx2).ExistsForUpdateAsync("Shared Name Party", id2);

            exists.Should().BeTrue();
        }
    }
}
