using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for MaintenanceDepartmentValidationRepository.
    /// Validates HasLinkedDepartmentAsync and HasActiveDepartmentAsync against
    /// CostCenter, MachineGroup, Feeder, WorkCenter, PreventiveSchedulerHeader,
    /// ActivityMaster, and MaintenanceRequest tables.
    /// Tests use CostCenter as a representative dependent table.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceDepartmentValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceDepartmentValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceDepartmentValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceDepartmentValidationRepository(conn);
        }

        private async Task SeedCostCenterAsync(
            int departmentId,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = $"CC_DEP_{departmentId}",
                CostCenterName = $"CostCenter Dep {departmentId}",
                UnitId = 1,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.CostCenter.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedDepartmentAsync ---

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_CostCenter_Uses_DepartmentId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 10);

            var result = await CreateRepo().HasLinkedDepartmentAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedDepartmentAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 20, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedDepartmentAsync(20);

            result.Should().BeFalse();
        }

        // --- HasActiveDepartmentAsync ---

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_True_When_CostCenter_Is_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 30, active: BaseEntity.Status.Active);

            var result = await CreateRepo().HasActiveDepartmentAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_CostCenter_Is_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 40, active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().HasActiveDepartmentAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCostCenterAsync(departmentId: 50, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasActiveDepartmentAsync(50);

            result.Should().BeFalse();
        }
    }
}
