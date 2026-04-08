using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeAsync(ApplicationDbContext ctx, string code = "MT01", string desc = "Test Type")
        {
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return miscType.Id;
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Production.ProductionPackDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Production.ProductionPackHeader");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Production.LotMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private static Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "M001",
            string desc = "Test Misc",
            int sortOrder = 1) =>
            new()
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = desc,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "M002", "Desc Two", 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("M002");
            saved.Description.Should().Be("Desc Two");
            saved.MiscTypeId.Should().Be(mtId);
            saved.SortOrder.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "M003", "Audit"));
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
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "M004", "Original"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Updated";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            updated.Description.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(1, "GHOST");
            entity.Id = 99999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "M005", "Del"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "M006", "DelFlag"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- GET MAX SORT ORDER ---

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_For_MiscType()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);

            await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "S1", "A", sortOrder: 3));
            await CreateRepo(ctx).CreateAsync(BuildEntity(mtId, "S2", "B", sortOrder: 7));

            var max = await CreateRepo(ctx).GetMaxSortOrderAsync(mtId);

            max.Should().Be(7);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_NoRecords()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);

            var max = await CreateRepo(ctx).GetMaxSortOrderAsync(mtId);

            max.Should().Be(0);
        }
    }
}
