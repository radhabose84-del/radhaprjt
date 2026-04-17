using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemInventoryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemInventoryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemInventoryCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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
            var itemId = await SeedItemAsync("II1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).CreateAsync(new ItemInventory
            {
                ItemId = itemId, Weight = 5.5m, ReorderLevel = 10
            });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemInventory.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.ReorderLevel.Should().Be(10);
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IG1");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemInventory.Add(new ItemInventory { ItemId = itemId, ReorderLevel = 3 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId);

            result.Should().NotBeNull();
            result!.ReorderLevel.Should().Be(3);
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
            var itemId = await SeedItemAsync("IU_N1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpdateAsync(new ItemInventory
            {
                ItemId = itemId, ReorderLevel = 7
            });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemInventory.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.ReorderLevel.Should().Be(7);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes_When_Existing()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IU_E1");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemInventory.Add(new ItemInventory
                {
                    ItemId = itemId, ReorderLevel = 5, ReorderQty = 20
                });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpdateAsync(new ItemInventory
            {
                ItemId = itemId, ReorderLevel = 100, ReorderQty = 200
            });
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemInventory.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.ReorderLevel.Should().Be(100);
            reloaded.ReorderQty.Should().Be(200);
        }
    }
}
