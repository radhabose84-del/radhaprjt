using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;
using UsageTypeEntity = InventoryManagement.Domain.Entities.UsageType;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemUsageTypeMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemUsageTypeMappingCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemUsageTypeMappingCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearAndSeedAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
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
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UsageType]");
        }

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx)
        {
            var item = new ItemMaster
            {
                ItemCode = "UTM001", ItemName = "UsageType Test Item",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        private async Task<int> SeedUsageTypeAsync(ApplicationDbContext ctx)
        {
            var ut = new UsageTypeEntity
            {
                UsageTypeCode = "UT001", UsageTypeName = "General Use",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.UsageType.Add(ut);
            await ctx.SaveChangesAsync();
            return ut.Id;
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
        public async Task CreateAsync_Should_Persist_Mapping()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            var usageId = await SeedUsageTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).CreateAsync(
                new ItemUsageTypeMapping { ItemId = itemId, UsageTypeId = usageId, UnitId = 1 },
                CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemUsageTypeMapping.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.UsageTypeId.Should().Be(usageId);
        }
    }
}
