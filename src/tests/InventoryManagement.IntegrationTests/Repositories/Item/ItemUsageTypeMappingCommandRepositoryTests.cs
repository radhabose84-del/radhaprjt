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
    public sealed class ItemUsageTypeMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemUsageTypeMappingCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemUsageTypeMappingCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private async Task<int> SeedUsageTypeAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var u = new InventoryManagement.Domain.Entities.UsageType
            {
                UsageTypeCode = code,
                UsageTypeName = code,
                ModuleId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.UsageType.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Empty_When_None()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IUM_G1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Mapping()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IUM_C1");
            var utId = await SeedUsageTypeAsync("UT_C1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).CreateAsync(new ItemUsageTypeMapping
            {
                ItemId = itemId, UsageTypeId = utId, UnitId = 1
            }, CancellationToken.None);

            var saved = await ctx.ItemUsageTypeMapping.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdateAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IUM_U1");
            var utId = await SeedUsageTypeAsync("UT_U1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemUsageTypeMappingDto>
            {
                new() { UsageTypeId = utId, UnitId = 1 }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemUsageTypeMapping.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_Missing_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IUM_U2");
            var ut1 = await SeedUsageTypeAsync("UT_D1");
            var ut2 = await SeedUsageTypeAsync("UT_D2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemUsageTypeMapping.AddRange(
                    new ItemUsageTypeMapping { ItemId = itemId, UsageTypeId = ut1, UnitId = 1 },
                    new ItemUsageTypeMapping { ItemId = itemId, UsageTypeId = ut2, UnitId = 1 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemUsageTypeMappingDto>
            {
                new() { UsageTypeId = ut1, UnitId = 1 }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var saved = await ctx2.ItemUsageTypeMapping.AsNoTracking().Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].UsageTypeId.Should().Be(ut1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Ignore_Invalid_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IUM_INV");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemUsageTypeMappingDto>
            {
                new() { UsageTypeId = 0, UnitId = 1 },
                new() { UsageTypeId = 1, UnitId = 0 }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemUsageTypeMapping.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().BeEmpty();
        }
    }
}
