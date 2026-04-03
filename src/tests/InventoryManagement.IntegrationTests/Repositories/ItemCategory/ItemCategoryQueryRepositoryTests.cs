using Dapper;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.ItemCategory
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ItemGroupCommandRepository(ctx, _fixture.IpMock.Object);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = "IG_QRY",
                ItemGroupName = "Query Test Group",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedCategoryAsync(int itemGroupId, string name = "Test Category")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ItemCategoryCommandRepository(ctx);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemGroupId = itemGroupId,
                ItemCategoryName = name,
                IsGroup = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearCategoryTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[ItemCategory]");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearCategoryTableAsync();
            var groupId = await SeedGroupAsync();
            var id = await SeedCategoryAsync(groupId, "Electronics");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ItemCategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearCategoryTableAsync();
            var groupId = await SeedGroupAsync();
            var id = await SeedCategoryAsync(groupId, "To Delete");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await new ItemCategoryCommandRepository(ctx).DeleteAsync(id, entity);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllItemCategoryAsync_Should_Return_Seeded_Record()
        {
            await ClearCategoryTableAsync();
            var groupId = await SeedGroupAsync();
            await SeedCategoryAsync(groupId);

            var (items, total) = await CreateQueryRepo().GetAllItemCategoryAsync(1, 10, null!);

            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllItemCategoryAsync_Should_Exclude_SoftDeleted()
        {
            await ClearCategoryTableAsync();
            var groupId = await SeedGroupAsync();
            var id = await SeedCategoryAsync(groupId, "Deleted Cat");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.ItemCategory.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await new ItemCategoryCommandRepository(ctx).DeleteAsync(id, entity);

            var (items, total) = await CreateQueryRepo().GetAllItemCategoryAsync(1, 10, null!);

            total.Should().Be(0);
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing_Record()
        {
            await ClearCategoryTableAsync();
            var groupId = await SeedGroupAsync();
            var id = await SeedCategoryAsync(groupId);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_NonExistent()
        {
            await ClearCategoryTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeTrue();
        }
    }
}
