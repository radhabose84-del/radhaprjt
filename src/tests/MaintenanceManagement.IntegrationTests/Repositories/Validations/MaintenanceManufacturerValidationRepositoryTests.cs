using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for MaintenanceManufacturerValidationRepository.
    /// Validates HasLinkedManufacturerAsync and HasActiveManufacturerAsync against
    /// MachineGroup.Manufacturer column.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceManufacturerValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceManufacturerValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceManufacturerValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceManufacturerValidationRepository(conn);
        }

        private async Task SeedMachineGroupAsync(
            int manufacturerId,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = $"MG_Mfr_{manufacturerId}",
                Manufacturer = manufacturerId,
                UnitId = 1,
                DepartmentId = 1,
                PowerSource = false,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.MachineGroup.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedManufacturerAsync ---

        [Fact]
        public async Task HasLinkedManufacturerAsync_Should_Return_True_When_MachineGroup_Uses_Manufacturer()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineGroupAsync(manufacturerId: 10);

            var result = await CreateRepo().HasLinkedManufacturerAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedManufacturerAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedManufacturerAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedManufacturerAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineGroupAsync(manufacturerId: 20, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedManufacturerAsync(20);

            result.Should().BeFalse();
        }

        // --- HasActiveManufacturerAsync ---

        [Fact]
        public async Task HasActiveManufacturerAsync_Should_Return_True_When_MachineGroup_Is_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineGroupAsync(manufacturerId: 30, active: BaseEntity.Status.Active);

            var result = await CreateRepo().HasActiveManufacturerAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveManufacturerAsync_Should_Return_False_When_MachineGroup_Is_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineGroupAsync(manufacturerId: 40, active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().HasActiveManufacturerAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveManufacturerAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineGroupAsync(manufacturerId: 50, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasActiveManufacturerAsync(50);

            result.Should().BeFalse();
        }
    }
}
