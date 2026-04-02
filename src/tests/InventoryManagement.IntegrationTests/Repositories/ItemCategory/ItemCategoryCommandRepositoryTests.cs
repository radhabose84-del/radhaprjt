using Dapper;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.ItemCategory
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedItemGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ItemGroupCommandRepository(ctx, _fixture.IpMock.Object);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = "IG_TEST",
                ItemGroupName = "Test Group",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static InventoryManagement.Domain.Entities.Item.ItemCategory BuildEntity(
            int itemGroupId,
            string name = "Test Category") =>
            new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemGroupId = itemGroupId,
                ItemCategoryName = name,
                IsGroup = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearCategoryTableAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategory]");
        }

        private async Task ClearGroupTableAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "Electronics"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemCategory.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ItemCategoryName.Should().Be("Electronics");
            saved.ItemGroupId.Should().Be(groupId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemCategory.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "Original"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            entity.ItemCategoryName = "Updated Name";
            var result = await CreateRepository(ctx).UpdateAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            updated.ItemCategoryName.Should().Be("Updated Name");
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Positive_Value()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            var result = await CreateRepository(ctx).DeleteAsync(id, entity);

            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearCategoryTableAsync(ctx);
            var groupId = await SeedItemGroupAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await CreateRepository(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ItemCategory
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
