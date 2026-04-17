using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureItemGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemGroup.FirstOrDefaultAsync(g => g.ItemGroupCode == "IC_GRP");
            if (existing != null) return existing.Id;
            var g = new ItemGroup
            {
                UnitId = 1, ItemGroupCode = "IC_GRP", ItemGroupName = "Category Group",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<int> SeedItemCategoryAsync(string name = "Cat1", IsDelete deleted = IsDelete.NotDeleted)
        {
            var groupId = await EnsureItemGroupAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var c = new ItemCategory
            {
                ItemGroupId = groupId,
                ItemCategoryName = name,
                IsGroup = 0,
                IsActive = Status.Active,
                IsDeleted = deleted
            };
            await ctx.ItemCategory.AddAsync(c);
            await ctx.SaveChangesAsync();
            return c.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetCategoryByIdsAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id1 = await SeedItemCategoryAsync("Electronics");
            var id2 = await SeedItemCategoryAsync("Furniture");
            await SeedItemCategoryAsync("Extra");

            var result = await CreateRepo().GetCategoryByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCategoryByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetCategoryByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCategoryByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetCategoryByIdsAsync(null!);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCategoryByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedItemCategoryAsync("Keep");
            var id2 = await SeedItemCategoryAsync("Drop", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetCategoryByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }
    }
}
