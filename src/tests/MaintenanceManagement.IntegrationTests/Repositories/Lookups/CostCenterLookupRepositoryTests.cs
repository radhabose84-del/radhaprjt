using MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance;
using Microsoft.Data.SqlClient;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class CostCenterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CostCenterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CostCenterLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CostCenterLookupRepository(conn);
        }

        private async Task<int> SeedCostCenterAsync(
            string code = "CC001",
            string name = "Test Cost Center",
            int unitId = 1,
            int departmentId = 10,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = code,
                CostCenterName = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.CostCenter.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // ── GetByIdAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_Not_Found()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Matching_CostCenter()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedCostCenterAsync(code: "CC100", name: "Engineering", unitId: 5, departmentId: 42);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CostCenterId.Should().Be(id);
            result.CostCenterCode.Should().Be("CC100");
            result.CostCenterName.Should().Be("Engineering");
            result.UnitId.Should().Be(5);
            result.DepartmentId.Should().Be(42);
        }

        [Fact]
        public async Task GetByIdAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedCostCenterAsync(isDeleted: true);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // ── GetByIdsAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Matching_CostCenters()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedCostCenterAsync(code: "CC1", name: "Alpha");
            var id2 = await SeedCostCenterAsync(code: "CC2", name: "Beta");
            var id3 = await SeedCostCenterAsync(code: "CC3", name: "Gamma");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.CostCenterCode).Should().BeEquivalentTo(new[] { "CC1", "CC2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedCostCenterAsync(code: "CC-KEEP");
            var id2 = await SeedCostCenterAsync(code: "CC-DEL", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().ContainSingle().Which.CostCenterCode.Should().Be("CC-KEEP");
        }

        [Fact]
        public async Task GetByIdsAsync_Deduplicates_Input_Ids()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedCostCenterAsync();

            var result = await CreateRepo().GetByIdsAsync(new[] { id, id, id });

            result.Should().ContainSingle();
        }

        // ── GetAllCostCentersAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetAllCostCentersAsync_Returns_Active_NonDeleted_Ordered_By_Name()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(code: "CC-Z", name: "Zulu Center");
            await SeedCostCenterAsync(code: "CC-A", name: "Alpha Center");
            await SeedCostCenterAsync(code: "CC-M", name: "Mike Center");

            var result = await CreateRepo().GetAllCostCentersAsync();

            result.Should().HaveCount(3);
            result.Select(r => r.CostCenterName).Should().ContainInOrder("Alpha Center", "Mike Center", "Zulu Center");
        }

        [Fact]
        public async Task GetAllCostCentersAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(code: "CC-ACTIVE");
            await SeedCostCenterAsync(code: "CC-OFF", isActive: false);

            var result = await CreateRepo().GetAllCostCentersAsync();

            result.Should().ContainSingle().Which.CostCenterCode.Should().Be("CC-ACTIVE");
        }

        [Fact]
        public async Task GetAllCostCentersAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(code: "CC-KEEP");
            await SeedCostCenterAsync(code: "CC-DEL", isDeleted: true);

            var result = await CreateRepo().GetAllCostCentersAsync();

            result.Should().ContainSingle().Which.CostCenterCode.Should().Be("CC-KEEP");
        }

        [Fact]
        public async Task GetAllCostCentersAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllCostCentersAsync();

            result.Should().BeEmpty();
        }
    }
}
