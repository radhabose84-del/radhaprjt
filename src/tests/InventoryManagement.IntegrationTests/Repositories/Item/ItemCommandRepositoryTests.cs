using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static ItemMaster BuildEntity(string code = "ITC1", string name = "Item1", int? parentId = null) =>
            new()
            {
                ItemCode = code,
                ItemName = name,
                ParentItemId = parentId,
                IsOnSpot = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("ICR1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("ICR2", "Item2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);

            saved.ItemCode.Should().Be("ICR2");
            saved.ItemName.Should().Be("Item2");
        }

        [Fact]
        public async Task CreateAsync_Should_Default_IsHazardous_To_False()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("HAZ1", "NotHazardous"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);

            saved.IsHazardous.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_IsHazardous_True()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity("HAZ2", "HazardousItem");
            entity.IsHazardous = true;
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);

            saved.IsHazardous.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_Should_Default_OldItemId_To_Null()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("OID1", "NoLegacyId"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);

            saved.OldItemId.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_OldItemId_When_Set()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity("OID2", "WithLegacyId");
            entity.OldItemId = 9876;
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);

            saved.OldItemId.Should().Be(9876);
        }

        // --- GetTrackingAsync ---

        [Fact]
        public async Task GetTrackingAsync_Should_Return_Matching()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GET1", "Item"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetTrackingAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetTrackingAsync_Should_Return_Null_When_Missing()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetTrackingAsync(9999999);

            result.Should().BeNull();
        }

        // --- ExistsByCodeForCreateAsync ---

        [Fact]
        public async Task ExistsByCodeForCreateAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("DUP1"));

            var result = await CreateRepo(ctx).ExistsByCodeForCreateAsync("DUP1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeForCreateAsync_Should_Return_False_When_Missing()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsByCodeForCreateAsync("NOEXIST");

            result.Should().BeFalse();
        }

        // --- ExistsByCodeForUpdateAsync ---

        [Fact]
        public async Task ExistsByCodeForUpdateAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("SELF1"));

            var result = await CreateRepo(ctx).ExistsByCodeForUpdateAsync("SELF1", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByCodeForUpdateAsync_Should_Return_True_When_Other_Has_Same_Code()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id1 = await CreateRepo(ctx).CreateAsync(BuildEntity("CODE_X"));
            var id2 = await CreateRepo(ctx).CreateAsync(BuildEntity("CODE_Y"));

            var result = await CreateRepo(ctx).ExistsByCodeForUpdateAsync("CODE_X", id2);

            result.Should().BeTrue();
        }

        // --- GetChildIdsAsync ---

        [Fact]
        public async Task GetChildIdsAsync_Should_Return_Children()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var parentId = await CreateRepo(ctx).CreateAsync(BuildEntity("P1", "Parent"));
            await CreateRepo(ctx).CreateAsync(BuildEntity("C1", "Child1", parentId));
            await CreateRepo(ctx).CreateAsync(BuildEntity("C2", "Child2", parentId));

            var result = await CreateRepo(ctx).GetChildIdsAsync(parentId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetChildIdsAsync_Should_Exclude_SoftDeleted_Children()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var parentId = await CreateRepo(ctx).CreateAsync(BuildEntity("P2", "Parent"));
            var child1 = BuildEntity("CA1", "Alive", parentId);
            var child2 = BuildEntity("CD1", "Dead", parentId);
            child2.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).CreateAsync(child1);
            await CreateRepo(ctx).CreateAsync(child2);

            var result = await CreateRepo(ctx).GetChildIdsAsync(parentId);

            result.Should().HaveCount(1);
        }

        // --- UpdatePriceGroupForChildrenAsync ---

        [Fact]
        public async Task UpdatePriceGroupForChildrenAsync_Should_Set_Children_PriceGroup()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            // Seed a valid PriceGroup (FK target)
            var pg = new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                PriceGroupCode = "PG_UPD1",
                PriceGroupName = "For Update",
                Description = "x",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.PriceGroupMaster.AddAsync(pg);
            await ctx.SaveChangesAsync();
            var pgId = pg.Id;

            var parentId = await CreateRepo(ctx).CreateAsync(BuildEntity("PP1", "Parent"));
            await CreateRepo(ctx).CreateAsync(BuildEntity("CC1", "C1", parentId));
            await CreateRepo(ctx).CreateAsync(BuildEntity("CC2", "C2", parentId));
            ctx.ChangeTracker.Clear();

            var updated = await CreateRepo(ctx).UpdatePriceGroupForChildrenAsync(parentId, pgId);
            ctx.ChangeTracker.Clear();

            updated.Should().Be(2);
            var children = await ctx.ItemMaster.Where(x => x.ParentItemId == parentId).ToListAsync();
            children.Should().AllSatisfy(c => c.PriceGroupId.Should().Be(pgId));
        }

        // --- UpdateItemImageAsync ---

        [Fact]
        public async Task UpdateItemImageAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("IMG1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateItemImageAsync(id, "image.png");
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.ItemMaster.FirstAsync(x => x.Id == id);
            reloaded.ItemImage.Should().Be("image.png");
        }

        [Fact]
        public async Task UpdateItemImageAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateItemImageAsync(9999999, "x.png");

            result.Should().BeFalse();
        }

        // --- ExistsByNameSmartForCreateAsync ---

        [Fact]
        public async Task ExistsByNameSmartForCreateAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("N1", "MyItemName"));

            var result = await CreateRepo(ctx).ExistsByNameSmartForCreateAsync("MyItemName");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameSmartForCreateAsync_Should_Normalize_Name()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("N2", "MyItem-Name"));

            var result = await CreateRepo(ctx).ExistsByNameSmartForCreateAsync("My Item Name");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameSmartForCreateAsync_Should_Return_False_When_Empty()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsByNameSmartForCreateAsync("");

            result.Should().BeFalse();
        }

        // --- ExistsByNameSmartForUpdateAsync ---

        [Fact]
        public async Task ExistsByNameSmartForUpdateAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("U1", "UniqName"));

            var result = await CreateRepo(ctx).ExistsByNameSmartForUpdateAsync("UniqName", id);

            result.Should().BeFalse();
        }
    }
}
