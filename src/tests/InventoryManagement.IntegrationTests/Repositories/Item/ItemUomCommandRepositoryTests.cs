using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemUomCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemUomCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemUomCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task<int> SeedItemAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var i = new ItemMaster
            {
                ItemCode = code, ItemName = $"Item {code}",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemMaster.AddAsync(i);
            await ctx.SaveChangesAsync();
            return i.Id;
        }

        private async Task<int> SeedUomAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "IU_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "IU_T", Description = "IU T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "IU_MM");
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = "IU_MM", Description = "IU MM", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }

            var u = new InventoryManagement.Domain.Entities.UOM
            {
                Code = code, UOMName = code, UOMTypeId = misc.Id, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.UOMs.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Empty_When_None()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_G1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_U1");
            var uomId = await SeedUomAsync("KG_UOM1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemUomDto>
            {
                new() { ConversionUOMId = uomId, ConversionRate = 2.5m }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemUOMs.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].ConversionRate.Should().Be(2.5m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Row()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_U2");
            var uomId = await SeedUomAsync("KG_UOM2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemUOMs.Add(new ItemUOM { ItemId = itemId, ConversionUOMId = uomId, ConversionRate = 1m });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemUomDto>
            {
                new() { ConversionUOMId = uomId, ConversionRate = 9m }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemUOMs.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.ConversionRate.Should().Be(9m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_Missing_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_U3");
            var u1 = await SeedUomAsync("UKG_A");
            var u2 = await SeedUomAsync("UKG_B");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemUOMs.AddRange(
                    new ItemUOM { ItemId = itemId, ConversionUOMId = u1, ConversionRate = 1m },
                    new ItemUOM { ItemId = itemId, ConversionUOMId = u2, ConversionRate = 2m });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemUomDto>
            {
                new() { ConversionUOMId = u1, ConversionRate = 1m }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var saved = await ctx2.ItemUOMs.AsNoTracking().Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].ConversionUOMId.Should().Be(u1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Ignore_Rows_Without_Valid_UomId()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_INV");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemUomDto>
            {
                new() { ConversionUOMId = null, ConversionRate = 1m },
                new() { ConversionUOMId = 0, ConversionRate = 1m }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemUOMs.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().BeEmpty();
        }
    }
}
