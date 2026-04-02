using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemManufactureCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemManufactureCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemManufactureCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearAndSeedAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
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

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx)
        {
            var item = new ItemMaster
            {
                ItemCode = "MFG001", ItemName = "Manufacture Test Item",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        private async Task<int> SeedMiscMasterAsync(ApplicationDbContext ctx)
        {
            var type = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "MfgType", Description = "Manufacturing Type",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();

            var misc = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id, Code = "InHouse", Description = "In-House",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
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
            var miscId = await SeedMiscMasterAsync(ctx);

            ctx.ItemManufacture.Add(new ItemManufacture
            {
                ItemId = itemId, UnitId = 1, ManufacturingTypeId = miscId
            });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ManufacturingTypeId.Should().Be(miscId);
        }
    }
}
