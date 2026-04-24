using Microsoft.Data.SqlClient;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class QualityMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public QualityMasterLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QualityMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string name, bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.QualityMaster
            {
                QualityName = name,
                Description = name + " desc",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.QualityMaster.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllQualityMasterAsync_Returns_Ordered_By_Name()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Premium");
            await SeedAsync("Basic");

            var result = await CreateRepo().GetAllQualityMasterAsync();

            result.Select(r => r.QualityName).Should().ContainInOrder("Basic", "Premium");
        }

        [Fact]
        public async Task GetAllQualityMasterAsync_Excludes_Inactive_And_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Keep");
            await SeedAsync("Off", isActive: false);
            await SeedAsync("Gone", isDeleted: true);

            var result = await CreateRepo().GetAllQualityMasterAsync();

            result.Should().ContainSingle().Which.QualityName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllQualityMasterAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllQualityMasterAsync();

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
