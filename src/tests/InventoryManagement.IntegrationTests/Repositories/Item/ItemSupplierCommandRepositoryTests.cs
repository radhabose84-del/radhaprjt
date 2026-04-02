using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSupplierCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSupplierCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemSupplierCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearAndSeedAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSale]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemQuality]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemPurchase]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemInventory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemLogs]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("UPDATE [Inventory].[ItemMaster] SET ParentItemId = NULL; DELETE FROM [Inventory].[ItemMaster]");
        }

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx)
        {
            var item = new ItemMaster
            {
                ItemCode = "SUP001", ItemName = "Supplier Test Item",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Empty_When_NoRecords()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var results = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            ctx.ItemSupplier.Add(new ItemSupplier
            {
                ItemId = itemId, SupplierId = 100, UnitId = 1,
                SupplierPartNo = "SP-001", DefaultSupplier = true
            });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].SupplierId.Should().Be(100);
        }
    }
}
