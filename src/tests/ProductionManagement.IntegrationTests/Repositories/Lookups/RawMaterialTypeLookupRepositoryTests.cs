using Microsoft.Data.SqlClient;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class RawMaterialTypeLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public RawMaterialTypeLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RawMaterialTypeLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string code, string name, bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.RawMaterialType
            {
                RawMaterialTypeCode = code,
                RawMaterialTypeName = name,
                Description = name + " desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.RawMaterialType.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_Ordered_By_Name()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("R1", "Polyester");
            await SeedAsync("R2", "Cotton");

            var result = await CreateRepo().GetAllAsync();

            result.Select(r => r.RawMaterialTypeName).Should().ContainInOrder("Cotton", "Polyester");
        }

        [Fact]
        public async Task GetAllAsync_Excludes_Inactive_And_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("R1", "Keep");
            await SeedAsync("R2", "Off", isActive: false);
            await SeedAsync("R3", "Gone", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.RawMaterialTypeName.Should().Be("Keep");
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
            var id1 = await SeedAsync("R1", "A");
            var id2 = await SeedAsync("R2", "B");
            await SeedAsync("R3", "C");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.RawMaterialTypeCode).Should().BeEquivalentTo(new[] { "R1", "R2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("R1", "K");
            var gone = await SeedAsync("R2", "G", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }
    }
}
