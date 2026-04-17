using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class ItemLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedItemAsync(
            string code,
            string name,
            int? parentItemId = null,
            bool isOnSpot = false,
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var item = new ItemMaster
            {
                ItemCode = code,
                ItemName = name,
                ParentItemId = parentItemId,
                IsOnSpot = isOnSpot,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ItemMaster.AddAsync(item);
            await ctx.SaveChangesAsync();
            return item.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Items()
        {
            await ClearAsync();
            var id1 = await SeedItemAsync("I1", "Item 1");
            var id2 = await SeedItemAsync("I2", "Item 2");
            await SeedItemAsync("I3", "Item 3");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedItemAsync("K1", "Keep");
            var id2 = await SeedItemAsync("K2", "Drop", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }

        // --- GetVariantsByParentIdAsync ---

        [Fact]
        public async Task GetVariantsByParentIdAsync_Should_Return_Child_Items()
        {
            await ClearAsync();
            var parentId = await SeedItemAsync("P1", "Parent");
            await SeedItemAsync("V1", "Variant 1", parentItemId: parentId);
            await SeedItemAsync("V2", "Variant 2", parentItemId: parentId);

            var result = await CreateRepo().GetVariantsByParentIdAsync(parentId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetVariantsByParentIdAsync_Should_Return_Empty_When_NoVariants()
        {
            await ClearAsync();
            var parentId = await SeedItemAsync("P2", "Parent");

            var result = await CreateRepo().GetVariantsByParentIdAsync(parentId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetVariantsByParentIdAsync_Should_Exclude_Inactive_Variants()
        {
            await ClearAsync();
            var parentId = await SeedItemAsync("P3", "Parent");
            await SeedItemAsync("ACT", "Active Variant", parentItemId: parentId);
            await SeedItemAsync("INA", "Inactive Variant", parentItemId: parentId, active: Status.Inactive);

            var result = await CreateRepo().GetVariantsByParentIdAsync(parentId);

            result.Should().HaveCount(1);
            result[0].ItemCode.Should().Be("ACT");
        }
    }
}
