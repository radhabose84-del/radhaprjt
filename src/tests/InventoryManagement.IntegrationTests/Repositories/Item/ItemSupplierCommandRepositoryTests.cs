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
    public sealed class ItemSupplierCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSupplierCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSupplierCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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
        public async Task GetByItemIdAsync_Should_Return_Empty_When_None()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_G1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Seeded_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_G2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemSupplier.Add(new ItemSupplier { ItemId = itemId, SupplierId = 1, UnitId = 1, SupplierPartNo = "PN1" });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().ContainSingle();
            result[0].SupplierPartNo.Should().Be("PN1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_U1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemSupplierDto>
            {
                new() { SupplierId = 1, UnitId = 1, SupplierPartNo = "P1" }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemSupplier.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].SupplierPartNo.Should().Be("P1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Row()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_U2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemSupplier.Add(new ItemSupplier { ItemId = itemId, SupplierId = 1, UnitId = 1, SupplierPartNo = "OLD" });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemSupplierDto>
            {
                new() { SupplierId = 1, UnitId = 1, SupplierPartNo = "NEW" }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemSupplier.AsNoTracking().FirstAsync(x => x.ItemId == itemId);
            reloaded.SupplierPartNo.Should().Be("NEW");
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_Missing_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_U3");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemSupplier.AddRange(
                    new ItemSupplier { ItemId = itemId, SupplierId = 1, UnitId = 1 },
                    new ItemSupplier { ItemId = itemId, SupplierId = 2, UnitId = 1 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemSupplierDto>
            {
                new() { SupplierId = 1, UnitId = 1 }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var saved = await ctx2.ItemSupplier.AsNoTracking().Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].SupplierId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Ignore_Invalid_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IS_INV");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemSupplierDto>
            {
                new() { SupplierId = 0, UnitId = 1 },
                new() { SupplierId = 1, UnitId = 0 }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemSupplier.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().BeEmpty();
        }
    }
}
