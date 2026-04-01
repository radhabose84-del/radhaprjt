using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemVariantAttributeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantAttributeCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemVariantAttributeCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

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

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx)
        {
            var item = new ItemMaster
            {
                ItemCode = "VAR001", ItemName = "Variant Test Item", HasVariants = true,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        private async Task<(int variantBasedOnId, int attributeId)> SeedMiscForVariantAsync(ApplicationDbContext ctx)
        {
            var type1 = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VarBased", Description = "Variant Based On",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(type1);
            await ctx.SaveChangesAsync();

            var basedOn = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type1.Id, Code = "Color", Description = "Color",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(basedOn);
            await ctx.SaveChangesAsync();

            var attr = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type1.Id, Code = "Red", Description = "Red",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(attr);
            await ctx.SaveChangesAsync();

            return (basedOn.Id, attr.Id);
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Empty_When_NoAttributes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var results = await CreateRepo(ctx).GetForItemAsync(itemId);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task UpsertAttributesAsync_Should_Insert_New_Attributes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);
            var (basedOnId, attrId) = await SeedMiscForVariantAsync(ctx);
            ctx.ChangeTracker.Clear();

            var attrs = new List<VariantAttributeDto>
            {
                new() { VariantBasedOn = basedOnId, AttributeId = attrId, Order = 1 }
            };

            await CreateRepo(ctx).UpsertAttributesAsync(itemId, attrs);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemVariantAttribute.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().HaveCount(1);
            saved[0].AttributeId.Should().Be(attrId);
        }
    }
}
