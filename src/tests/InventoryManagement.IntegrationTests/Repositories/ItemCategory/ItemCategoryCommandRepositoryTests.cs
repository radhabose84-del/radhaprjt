using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemCategoryTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> EnsureItemGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemGroup.FirstOrDefaultAsync(g => g.ItemGroupCode == "ICC_GRP");
            if (existing != null) return existing.Id;
            var g = new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                UnitId = 1, ItemGroupCode = "ICC_GRP", ItemGroupName = "ItemCat Cmd Grp",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<InventoryManagement.Domain.Entities.Item.ItemCategory> BuildEntityAsync(
            string name = "Cat1",
            int? parentId = null)
        {
            var groupId = await EnsureItemGroupAsync();
            return new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemGroupId = groupId,
                ItemCategoryName = name,
                IsGroup = 0,
                ParentCategoryId = parentId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Cat_C1"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_RootCategoryId_To_Self_When_NoParent()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("RootCat"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemCategory.FirstAsync(x => x.Id == id);

            saved.RootCategoryId.Should().Be(id);
        }

        [Fact]
        public async Task CreateAsync_Should_Inherit_RootCategoryId_From_Parent()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var rootId = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("ParentCat"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();
            var childId = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("ChildCat", parentId: rootId), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            var child = await ctx.ItemCategory.FirstAsync(x => x.Id == childId);
            child.RootCategoryId.Should().Be(rootId);
        }

        [Fact]
        public async Task CreateAsync_Should_Insert_ModuleAssignments()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(
                await BuildEntityAsync("ModCat"), new List<int> { 1, 2, 2 }, new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>()); // dedup
            ctx.ChangeTracker.Clear();

            var modules = await ctx.ItemCategoryModule.Where(m => m.ItemCategoryId == id).ToListAsync();
            modules.Should().HaveCount(2);
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("UPD1"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("Updated");
            updated.IsActive = Status.Inactive;
            var result = await CreateRepo(ctx).UpdateAsync(id, updated, new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var reloaded = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            reloaded.ItemCategoryName.Should().Be("Updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(9999999, await BuildEntityAsync("Ghost"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());

            result.Should().Be(-1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_ModuleAssignments()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Mod"), new List<int> { 1, 2 }, new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(id, await BuildEntityAsync("Mod"), new List<int> { 3 }, new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            var modules = await ctx.ItemCategoryModule.Where(m => m.ItemCategoryId == id).ToListAsync();
            modules.Should().HaveCount(1);
            modules[0].ModuleId.Should().Be(3);
        }

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DEL1"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("DEL1");
            entity.IsDeleted = IsDelete.Deleted;
            var result = await CreateRepo(ctx).DeleteAsync(id, entity);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_IsDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DEL2"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("DEL2");
            entity.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("Ghost");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().Be(-1);
        }

        // --- ExistsByNameAsync ---

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_For_Existing()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("ExName"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());

            var result = await CreateRepo(ctx).ExistsByNameAsync("ExName");

            result.Should().BeTrue();
        }

        // --- IsNameDuplicateAsync ---

        [Fact]
        public async Task IsNameDuplicateAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SelfN"), new List<int>(), new List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>());

            var result = await CreateRepo(ctx).IsNameDuplicateAsync("SelfN", id);

            result.Should().BeFalse();
        }
    }
}
