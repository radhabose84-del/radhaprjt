using MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance;
using Microsoft.Data.SqlClient;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentValidationLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentValidationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepartmentValidationLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepartmentValidationLookupRepository(conn);
        }

        private async Task SeedCostCenterAsync(int departmentId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.CostCenter.Add(new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = $"CC-D{departmentId}",
                CostCenterName = $"CC for Dept {departmentId}",
                UnitId = 1,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task IsDepartmentUsedAsync_Returns_True_When_Referenced_By_CostCenter()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 42);

            var result = await CreateRepo().IsDepartmentUsedAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsDepartmentUsedAsync_Returns_False_When_Not_Referenced()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 42);

            var result = await CreateRepo().IsDepartmentUsedAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsDepartmentUsedAsync_Returns_False_When_NoCostCenters()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().IsDepartmentUsedAsync(42);

            result.Should().BeFalse();
        }
    }
}
