using Microsoft.Data.SqlClient;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class ProcessMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProcessMasterLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProcessMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string name, bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.ProcessMaster
            {
                ProcessName = name,
                CombingRequired = false,
                Description = name + " desc",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.ProcessMaster.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllProcessMasterAsync_Returns_Ordered_By_Name()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Spinning");
            await SeedAsync("Carding");

            var result = await CreateRepo().GetAllProcessMasterAsync();

            result.Select(r => r.ProcessName).Should().ContainInOrder("Carding", "Spinning");
        }

        [Fact]
        public async Task GetAllProcessMasterAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Keep");
            await SeedAsync("Hidden", isActive: false);

            var result = await CreateRepo().GetAllProcessMasterAsync();

            result.Should().ContainSingle().Which.ProcessName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllProcessMasterAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Keep");
            await SeedAsync("Gone", isDeleted: true);

            var result = await CreateRepo().GetAllProcessMasterAsync();

            result.Should().ContainSingle().Which.ProcessName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllProcessMasterAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllProcessMasterAsync();

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
            var id1 = await SeedAsync("A");
            var id2 = await SeedAsync("B");
            await SeedAsync("C");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.ProcessName).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("K");
            var gone = await SeedAsync("G", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }
    }
}
