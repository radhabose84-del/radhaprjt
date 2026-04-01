using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemPurchaseCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemPurchaseCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemPurchaseCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearAndSeedAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemPurchase]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSale]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemQuality]");
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
                ItemCode = "PUR001", ItemName = "Purchase Test Item",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_ItemPurchase()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).CreateAsync(new ItemPurchase
            {
                ItemId = itemId, LeadTimeDays = 14, GrProcessingTimeDays = 2, AutomaticPo = true
            });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemPurchase.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.LeadTimeDays.Should().Be(14);
            saved.AutomaticPo.Should().BeTrue();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Null_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByItemIdAsync(99999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            await CreateRepo(ctx).CreateAsync(new ItemPurchase { ItemId = itemId, LeadTimeDays = 7 });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepo(ctx).GetByItemIdAsync(itemId);
            entity!.LeadTimeDays = 21;
            await CreateRepo(ctx).UpdateAsync(entity);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemPurchase.FirstAsync(x => x.ItemId == itemId);
            saved.LeadTimeDays.Should().Be(21);
        }
    }
}
