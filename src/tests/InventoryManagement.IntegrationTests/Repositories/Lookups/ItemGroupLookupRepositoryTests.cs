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
    public sealed class ItemGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedItemGroupAsync(string code = "IG1", Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new ItemGroup
            {
                UnitId = 1, ItemGroupCode = code, ItemGroupName = $"Group {code}",
                IsActive = active, IsDeleted = deleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllItemGroupsAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedItemGroupAsync("IG-A");

            var result = await CreateRepo().GetAllItemGroupsAsync();

            result.Should().Contain(g => g.ItemGroupCode == "IG-A");
        }

        [Fact]
        public async Task GetAllItemGroupsAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedItemGroupAsync("INACT", active: Status.Inactive);

            var result = await CreateRepo().GetAllItemGroupsAsync();

            result.Should().NotContain(g => g.ItemGroupCode == "INACT");
        }

        [Fact]
        public async Task GetAllItemGroupsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedItemGroupAsync("DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllItemGroupsAsync();

            result.Should().NotContain(g => g.ItemGroupCode == "DEL");
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id1 = await SeedItemGroupAsync("A");
            var id2 = await SeedItemGroupAsync("B");
            await SeedItemGroupAsync("C");

            var result = await CreateRepo().GetItemGroupsByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetItemGroupsByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedItemGroupAsync("K1");
            var id2 = await SeedItemGroupAsync("K2", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetItemGroupsByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }
    }
}
