using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemInventoryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemInventoryCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemInventoryCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemInventory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSale]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemQuality]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemPurchase]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemLogs]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("UPDATE [Inventory].[ItemMaster] SET ParentItemId = NULL; DELETE FROM [Inventory].[ItemMaster]");
        }

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx)
        {
            var item = new ItemMaster
            {
                ItemCode = "INV001",
                ItemName = "Inventory Test Item",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_ItemInventory()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            ctx.ChangeTracker.Clear();

            var inventory = new ItemInventory
            {
                ItemId = itemId,
                Weight = 1.5m,
                ShelfLife = 365,
                ReorderLevel = 50,
                ReorderQty = 100,
                SafetyStock = 20,
                AllowNegativeStock = false,
                BatchManagement = true,
                ApplyBatchNumber = true
            };
            await CreateRepo(ctx).CreateAsync(inventory);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemInventory.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.Weight.Should().Be(1.5m);
            saved.ShelfLife.Should().Be(365);
            saved.BatchManagement.Should().BeTrue();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Entity()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            await CreateRepo(ctx).CreateAsync(new ItemInventory { ItemId = itemId, Weight = 3m });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByItemIdAsync(itemId);

            result.Should().NotBeNull();
            result!.Weight.Should().Be(3m);
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Null_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepo(ctx).GetByItemIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            await CreateRepo(ctx).CreateAsync(new ItemInventory { ItemId = itemId, Weight = 1m });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepo(ctx).GetByItemIdAsync(itemId);
            entity!.Weight = 9.9m;
            entity.SafetyStock = 999;
            await CreateRepo(ctx).UpdateAsync(entity);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemInventory.FirstAsync(x => x.ItemId == itemId);
            saved.Weight.Should().Be(9.9m);
            saved.SafetyStock.Should().Be(999);
        }
    }
}
