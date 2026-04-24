using Microsoft.Data.SqlClient;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class PackTypeLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PackTypeLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string code, string name, bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.PackType
            {
                PackTypeCode = code,
                PackTypeName = name,
                NetWeight = 5m,
                TareWeight = 1m,
                GrossWeight = 6m,
                ConesPerBag = 10,
                ProductionAllowed = true,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.PackType.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_Ordered_By_Name_With_Weights()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("PT1", "Bale");
            await SeedAsync("PT2", "Bag");

            var result = await CreateRepo().GetAllAsync();

            result.Select(r => r.PackTypeName).Should().ContainInOrder("Bag", "Bale");
            result.First().NetWeight.Should().Be(5m);
            result.First().TareWeight.Should().Be(1m);
            result.First().GrossWeight.Should().Be(6m);
            result.First().ConesPerBag.Should().Be(10);
        }

        [Fact]
        public async Task GetAllAsync_Excludes_Inactive_And_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("PT1", "Keep");
            await SeedAsync("PT2", "Off", isActive: false);
            await SeedAsync("PT3", "Gone", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.PackTypeName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Empty_Input_Returns_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Null_Input_Returns_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Matching_Records()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedAsync("PT1", "A");
            var id2 = await SeedAsync("PT2", "B");
            await SeedAsync("PT3", "C");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.PackTypeCode).Should().BeEquivalentTo(new[] { "PT1", "PT2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("PT1", "K");
            var gone = await SeedAsync("PT2", "G", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }
    }
}
