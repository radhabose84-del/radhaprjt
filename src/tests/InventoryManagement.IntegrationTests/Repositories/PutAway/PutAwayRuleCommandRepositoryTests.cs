using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.Templates;
using ItemGroupEntity = InventoryManagement.Domain.Entities.Item.ItemGroup;
using ItemCategoryEntity = InventoryManagement.Domain.Entities.Item.ItemCategory;

namespace InventoryManagement.IntegrationTests.Repositories.PutAway
{
    [Collection("DatabaseCollection")]
    public sealed class PutAwayRuleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PutAwayRuleCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PutAwayRuleCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        private async Task<(int groupId, int categoryId)> SeedGroupAndCategoryAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new ItemGroupEntity
            {
                UnitId = 1, ItemGroupCode = "GRP001", ItemGroupName = "Test Group",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemGroup.Add(group);
            await ctx.SaveChangesAsync();

            var category = new ItemCategoryEntity
            {
                ItemGroupId = group.Id, ItemCategoryName = "Test Category",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemCategory.Add(category);
            await ctx.SaveChangesAsync();

            return (group.Id, category.Id);
        }

        private static PutAwayRule BuildRule(int groupId, int categoryId, int warehouseId = 1) =>
            new PutAwayRule
            {
                UnitId = 1,
                ItemGroupId = groupId,
                ItemCategoryId = categoryId,
                WarehouseId = warehouseId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted,
                Strategies = new List<PutAwayStrategy>()
            };

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, categoryId) = await SeedGroupAndCategoryAsync();

            var id = await CreateRepository(ctx).CreateAsync(BuildRule(groupId, categoryId), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, categoryId) = await SeedGroupAndCategoryAsync();

            var id = await CreateRepository(ctx).CreateAsync(BuildRule(groupId, categoryId, 5), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PutAwayRule>().FirstOrDefaultAsync(x => x.Id == id);
            saved.Should().NotBeNull();
            saved!.WarehouseId.Should().Be(5);
            saved.ItemGroupId.Should().Be(groupId);
            saved.ItemCategoryId.Should().Be(categoryId);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, categoryId) = await SeedGroupAndCategoryAsync();

            var id = await CreateRepository(ctx).CreateAsync(BuildRule(groupId, categoryId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.Set<PutAwayRule>().Include(r => r.Strategies).FirstAsync(x => x.Id == id);
            var repo = CreateRepository(ctx2);
            await repo.SoftDeleteAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var deleted = await ctx3.Set<PutAwayRule>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task ExistsScopeAsync_Should_Return_True_When_Duplicate_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, categoryId) = await SeedGroupAndCategoryAsync();

            await CreateRepository(ctx).CreateAsync(BuildRule(groupId, categoryId, 10), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var exists = await CreateRepository(ctx2).ExistsScopeAsync(1, 10, groupId, categoryId, null);

            exists.Should().BeTrue();
        }
    }
}
