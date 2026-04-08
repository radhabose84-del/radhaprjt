using Dapper;
using Microsoft.Data.SqlClient;
using LogisticsManagement.Domain.Common;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Repositories.MiscMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static MiscMasterCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeAsync(string code = "FMODE", string description = "Freight Mode")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private static Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "MM001",
            string description = "Test MiscMaster",
            int sortOrder = 1) =>
            new()
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Logistics.FreightMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM001", "Test Desc", 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeId.Should().Be(miscTypeId);
            saved.Code.Should().Be("MM001");
            saved.Description.Should().Be("Test Desc");
            saved.SortOrder.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Updated Description";
            entity.SortOrder = 10;
            var result = await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscMaster.FirstAsync(x => x.Id == id);
            updated.Description.Should().Be("Updated Description");
            updated.SortOrder.Should().Be(10);
            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "ORIG01"));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Changed Name Only";
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscMaster.FirstAsync(x => x.Id == id);
            updated.Code.Should().Be("ORIG01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(miscTypeId);
            entity.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_Modified_Audit_Fields()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Modified";
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscMaster.FirstAsync(x => x.Id == id);
            updated.ModifiedBy.Should().Be(1);
            updated.ModifiedByName.Should().Be("test-user");
            updated.ModifiedDate.Should().NotBeNull();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var deleted = await ctx3.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- GET MAX SORT ORDER ---

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_Empty()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var maxOrder = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            maxOrder.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_SortOrder()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM001", "First", 5));
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM002", "Second", 15));
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM003", "Third", 10));

            var maxOrder = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            maxOrder.Should().Be(15);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildEntity(miscTypeId, "MM001", "First", 5));
            var id2 = await repo.CreateAsync(BuildEntity(miscTypeId, "MM002", "Second", 20));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id2, CancellationToken.None);

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var maxOrder = await CreateRepository(ctx3).GetMaxSortOrderAsync(miscTypeId);

            maxOrder.Should().Be(5);
        }
    }
}
