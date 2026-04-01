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
    public sealed class ItemVariantValueCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantValueCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemVariantValueCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

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

        private async Task<int> SeedItemMasterAsync(ApplicationDbContext ctx, string code = "VV001")
        {
            var item = new ItemMaster
            {
                ItemCode = code, ItemName = $"Variant Value Test {code}", HasVariants = true,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        private async Task<int> SeedAttributeAsync(ApplicationDbContext ctx, int itemId)
        {
            var type = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VBased", Description = "Variant Based",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();

            var basedOn = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id, Code = "Size", Description = "Size",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var attr = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id, Code = "SizeAttr", Description = "Size Attribute",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.MiscMaster.AddRange(basedOn, attr);
            await ctx.SaveChangesAsync();

            var va = new ItemVariantAttribute
            {
                ItemId = itemId, VariantBasedOn = basedOn.Id, AttributeId = attr.Id, Order = 1
            };
            ctx.ItemVariantAttribute.Add(va);
            await ctx.SaveChangesAsync();
            return va.Id;
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Empty_When_NoValues()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var itemId = await SeedItemMasterAsync(ctx);

            var results = await CreateRepo(ctx).GetForItemAsync(itemId);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Seeded_Values()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAndSeedAsync(ctx);
            var templateId = await SeedItemMasterAsync(ctx, "VV002");
            var childId = await SeedItemMasterAsync(ctx, "VV003");
            var attrId = await SeedAttributeAsync(ctx, templateId);

            ctx.ItemVariantValue.Add(new ItemVariantValue
            {
                ItemId = templateId, VariantAttributeId = attrId,
                OptionValue = "Large", ParentItemId = childId
            });
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetForItemAsync(templateId);

            results.Should().HaveCount(1);
        }
    }
}
