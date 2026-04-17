using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSaleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSaleCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSaleCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Add_To_DbContext()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).CreateAsync(new ItemSale { ItemId = itemId, MinQuantity = 5 });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemSale.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.MinQuantity.Should().Be(5);
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("ISG");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemSale.Add(new ItemSale { ItemId = itemId, PackageQuantity = 12 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId);

            result.Should().NotBeNull();
            result!.PackageQuantity.Should().Be(12);
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Null_When_Missing()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByItemIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Create_When_NotFound()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("ISU_N");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpdateAsync(new ItemSale { ItemId = itemId, MinQuantity = 1 });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemSale.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.MinQuantity.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes_When_Existing()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("ISU_E");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemSale.Add(new ItemSale { ItemId = itemId, MinQuantity = 1 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpdateAsync(new ItemSale
            {
                ItemId = itemId, MinQuantity = 50, PackageQuantity = 10, Discount = true
            });
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemSale.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.MinQuantity.Should().Be(50);
            reloaded.PackageQuantity.Should().Be(10);
        }
    }
}
