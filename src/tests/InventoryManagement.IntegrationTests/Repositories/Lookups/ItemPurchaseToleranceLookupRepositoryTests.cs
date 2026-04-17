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
    public sealed class ItemPurchaseToleranceLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemPurchaseToleranceLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemPurchaseToleranceLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedItemAsync(string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var item = new ItemMaster
            {
                ItemCode = code, ItemName = name,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemMaster.AddAsync(item);
            await ctx.SaveChangesAsync();
            return item.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Items_With_Null_ToleranceFields_When_NoInventory()
        {
            await ClearAsync();
            var id1 = await SeedItemAsync("IPT_A", "Alpha");
            var id2 = await SeedItemAsync("IPT_B", "Beta");

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
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await ClearAsync();
            var id = await SeedItemAsync("IPT_X", "X");

            var result = await CreateRepo().GetByIdsAsync(new[] { id, 0, -1 });

            result.Should().HaveCount(1);
            result[0].ItemId.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Order_By_ItemCode()
        {
            await ClearAsync();
            var idZ = await SeedItemAsync("IPT_Z", "Zeta");
            var idA = await SeedItemAsync("IPT_A2", "Alpha2");
            var idM = await SeedItemAsync("IPT_M", "Mid");

            var result = await CreateRepo().GetByIdsAsync(new[] { idZ, idA, idM });

            var codes = result.Select(r => r.ItemCode).ToList();
            codes.IndexOf("IPT_A2").Should().BeLessThan(codes.IndexOf("IPT_M"));
            codes.IndexOf("IPT_M").Should().BeLessThan(codes.IndexOf("IPT_Z"));
        }
    }
}
