using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemManufactureCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemManufactureCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemManufactureCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private async Task<int> SeedMiscMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "IM_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "IM_T", Description = "IM Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code);
            if (misc != null) return misc.Id;
            misc = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id, Code = code, Description = code, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Empty_When_No_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IM_G1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByItemIdAsync_Should_Return_Seeded_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IM_G2");
            var miscId = await SeedMiscMasterAsync("IM_MT1");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemManufacture.Add(new ItemManufacture { ItemId = itemId, UnitId = 1, ManufacturingTypeId = miscId });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetByItemIdAsync(itemId, CancellationToken.None);

            result.Should().ContainSingle();
            result[0].ManufacturingTypeId.Should().Be(miscId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IM_U1");
            var miscId = await SeedMiscMasterAsync("IM_MT2");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemManufactureDto>
            {
                new() { UnitId = 1, ManufacturingTypeId = miscId }
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemManufacture.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_Missing_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IM_U2");
            var m1 = await SeedMiscMasterAsync("IM_RM1");
            var m2 = await SeedMiscMasterAsync("IM_RM2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemManufacture.AddRange(
                    new ItemManufacture { ItemId = itemId, UnitId = 1, ManufacturingTypeId = m1 },
                    new ItemManufacture { ItemId = itemId, UnitId = 1, ManufacturingTypeId = m2 });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var rows = new List<ItemManufactureDto>
            {
                new() { UnitId = 1, ManufacturingTypeId = m1 }
            };
            await CreateRepo(ctx2).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var saved = await ctx2.ItemManufacture.AsNoTracking().Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].ManufacturingTypeId.Should().Be(m1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_ItemMaster_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).UpdateAsync(
                9999999, new List<ItemManufactureDto>(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*ItemMaster does not exist*");
        }

        [Fact]
        public async Task UpdateAsync_Should_Ignore_Invalid_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IM_INV");
            await using var ctx = _fixture.CreateFreshDbContext();

            var rows = new List<ItemManufactureDto>
            {
                new() { UnitId = 0, ManufacturingTypeId = 1 }, // UnitId = 0 is invalid
                new() { UnitId = 1, ManufacturingTypeId = 0 }  // ManufacturingTypeId = 0 is invalid
            };
            await CreateRepo(ctx).UpdateAsync(itemId, rows, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemManufacture.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().BeEmpty();
        }
    }
}
