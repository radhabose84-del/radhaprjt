using Microsoft.Data.SqlClient;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class CertificationMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CertificationMasterLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CertificationMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string name, bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.CertificationMaster
            {
                CertificationName = name,
                Description = name + " desc",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.CertificationMaster.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllCertificationMasterAsync_Returns_Active_NonDeleted_Ordered_By_Name()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("ISO 9001");
            await SeedAsync("AATCC");

            var result = await CreateRepo().GetAllCertificationMasterAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.CertificationName).Should().ContainInOrder("AATCC", "ISO 9001");
        }

        [Fact]
        public async Task GetAllCertificationMasterAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Keep");
            await SeedAsync("Hidden", isActive: false);

            var result = await CreateRepo().GetAllCertificationMasterAsync();

            result.Should().ContainSingle().Which.CertificationName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllCertificationMasterAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("Keep");
            await SeedAsync("Gone", isDeleted: true);

            var result = await CreateRepo().GetAllCertificationMasterAsync();

            result.Should().ContainSingle().Which.CertificationName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllCertificationMasterAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllCertificationMasterAsync();

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
            var id1 = await SeedAsync("ISO");
            var id2 = await SeedAsync("OEKO");
            await SeedAsync("GOTS");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.CertificationName).Should().BeEquivalentTo(new[] { "ISO", "OEKO" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("Keep");
            var gone = await SeedAsync("Gone", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }

        [Fact]
        public async Task GetByIdsAsync_Includes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync("Inactive", isActive: false);

            var result = await CreateRepo().GetByIdsAsync(new[] { id });

            result.Should().ContainSingle();
        }
    }
}
