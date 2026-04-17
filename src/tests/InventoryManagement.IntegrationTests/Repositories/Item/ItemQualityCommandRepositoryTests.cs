using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemQualityCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemQualityCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemQualityCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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
            var itemId = await SeedItemAsync("IQC1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).CreateAsync(new ItemQuality
            {
                ItemId = itemId, InspectionRequired = true
            });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemQuality.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.InspectionRequired.Should().BeTrue();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IQG");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemQuality.Add(new ItemQuality { ItemId = itemId, InspLotProcessingTime = 4 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId);

            result.Should().NotBeNull();
            result!.InspLotProcessingTime.Should().Be(4);
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
            var itemId = await SeedItemAsync("IQU_N");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpdateAsync(new ItemQuality { ItemId = itemId, InspectionRequired = true });
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemQuality.FirstOrDefaultAsync(x => x.ItemId == itemId);
            saved.Should().NotBeNull();
            saved!.InspectionRequired.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes_When_Existing()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IQU_E");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemQuality.Add(new ItemQuality { ItemId = itemId, InspectionRequired = false });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpdateAsync(new ItemQuality { ItemId = itemId, InspectionRequired = true });
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemQuality.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.InspectionRequired.Should().BeTrue();
        }
    }
}
