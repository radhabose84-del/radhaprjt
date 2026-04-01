using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemVariantValueQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantValueQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemVariantValueQueryRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearAndSeedAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSale]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemQuality]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemPurchase]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemInventory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemLogs]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("UPDATE [Inventory].[ItemMaster] SET ParentItemId = NULL; DELETE FROM [Inventory].[ItemMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx, string code = "VQ001")
        {
            var item = new ItemMaster
            {
                ItemCode = code, ItemName = $"VQ Test {code}", HasVariants = true,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        [Fact]
        public async Task GetForItemGroupedAsync_Should_Return_Empty_When_NoValues()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var result = await CreateRepo(ctx).GetForItemGroupedAsync(itemId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExistingChildComboKeysAsync_Should_Return_Empty_When_NoChildren()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var result = await CreateRepo(ctx).GetExistingChildComboKeysAsync(itemId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExistingChildCombosWithIdsAsync_Should_Return_Empty_When_NoChildren()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var result = await CreateRepo(ctx).GetExistingChildCombosWithIdsAsync(itemId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Empty_When_NoValues()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var result = await CreateRepo(ctx).GetForItemAsync(itemId);

            result.Should().BeEmpty();
        }
    }
}
