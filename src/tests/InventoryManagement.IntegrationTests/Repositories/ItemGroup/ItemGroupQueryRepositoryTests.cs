using Dapper;
using Microsoft.Data.SqlClient;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;

namespace InventoryManagement.IntegrationTests.Repositories.ItemGroup
{
    [Collection("DatabaseCollection")]
    public sealed class ItemGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemGroupQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "IG_QRY001", string name = "Query Test Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ItemGroupCommandRepository(ctx, _fixture.IpMock.Object);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = code,
                ItemGroupName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllItemGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllItemGroupAsync(1, 10, null);

            ((IEnumerable<dynamic>)items).Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllItemGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("IG_DEL1", "Delete Me");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ItemGroupCommandRepository(ctx, _fixture.IpMock.Object).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.Item.ItemGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllItemGroupAsync(1, 10, null);

            ((IEnumerable<dynamic>)items).Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllItemGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("IG_ALPHA", "Alpha Group");
            await SeedEntityAsync("IG_BETA", "Beta Group");

            var (items, _) = await CreateQueryRepo().GetAllItemGroupAsync(1, 10, "Alpha");

            ((IEnumerable<dynamic>)items).Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("IG_ID1", "Get By Id Group");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ItemGroupCode.Should().Be("IG_ID1");
            result.ItemGroupName.Should().Be("Get By Id Group");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("IG_DEL2", "Soft Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ItemGroupCommandRepository(ctx, _fixture.IpMock.Object).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.Item.ItemGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetItemGroupAutoCompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("IG_AC1", "Autocomplete Group");

            var results = await CreateQueryRepo().GetItemGroupAutoCompleteAsync("Autocomplete");

            results.Should().NotBeEmpty();
            results[0].ItemGroupName.Should().Be("Autocomplete Group");
        }
    }
}
