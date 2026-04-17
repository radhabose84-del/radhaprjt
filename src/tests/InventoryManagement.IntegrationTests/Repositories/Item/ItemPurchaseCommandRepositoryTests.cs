using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemPurchaseCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemPurchaseCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemPurchaseCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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
            var itemId = await SeedItemAsync("IP1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).CreateAsync(new ItemPurchase
            {
                ItemId = itemId, LeadTimeDays = 10, AutomaticPo = true
            });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemPurchase.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.LeadTimeDays.Should().Be(10);
            saved.AutomaticPo.Should().BeTrue();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IPG");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemPurchase.Add(new ItemPurchase { ItemId = itemId, LeadTimeDays = 3 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId);

            result.Should().NotBeNull();
            result!.LeadTimeDays.Should().Be(3);
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
            var itemId = await SeedItemAsync("IPU_N");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpdateAsync(new ItemPurchase { ItemId = itemId, LeadTimeDays = 7 });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemPurchase.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.LeadTimeDays.Should().Be(7);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes_When_Existing()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IPU_E");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemPurchase.Add(new ItemPurchase { ItemId = itemId, LeadTimeDays = 5 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpdateAsync(new ItemPurchase { ItemId = itemId, LeadTimeDays = 99, AutomaticPo = true });
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemPurchase.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.LeadTimeDays.Should().Be(99);
            reloaded.AutomaticPo.Should().BeTrue();
        }
    }
}
