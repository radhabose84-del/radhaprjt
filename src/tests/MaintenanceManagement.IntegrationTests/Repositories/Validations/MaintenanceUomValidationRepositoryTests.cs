using Dapper;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for MaintenanceUomValidationRepository.
    /// Validates HasLinkedUomAsync and HasActiveUomAsync against
    /// MachineMaster.UomId column.
    /// MachineMaster has deep FK chains, so we use raw SQL with NOCHECK constraints
    /// to seed minimal test data.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceUomValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceUomValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceUomValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceUomValidationRepository(conn);
        }

        /// <summary>
        /// Seeds a minimal MachineMaster row via raw SQL with FK constraints temporarily disabled.
        /// MachineMaster has many FK dependencies (MachineGroup, ShiftMaster, CostCenter, WorkCenter, MiscMaster for LineNo).
        /// </summary>
        private async Task SeedMachineMasterWithUomAsync(
            int uomId,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Disable FK checks temporarily
            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[MachineMaster] NOCHECK CONSTRAINT ALL;");

            await conn.ExecuteAsync(@"
                INSERT INTO [Maintenance].[MachineMaster]
                    (MachineCode, MachineName, MachineGroupId, UnitId, ProductionCapacity,
                     UomId, ShiftMasterId, CostCenterId, WorkCenterId, InstallationDate,
                     AssetId, [LineNo], IsProductionMachine, IsActive, IsDeleted,
                     CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                VALUES
                    (@Code, @Name, 1, 1, 0,
                     @UomId, 1, 1, 1, GETUTCDATE(),
                     0, 1, 0, @IsActive, @IsDeleted,
                     1, 'test-user', '127.0.0.1', GETUTCDATE());",
                new
                {
                    Code = $"MM_UOM_{uomId}",
                    Name = $"Machine UOM {uomId}",
                    UomId = uomId,
                    IsActive = isActive,
                    IsDeleted = isDeleted
                });

            // Re-enable FK checks (without WITH CHECK to avoid FK violation on dummy FK values)
            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[MachineMaster] CHECK CONSTRAINT ALL;");
        }

        // --- HasLinkedUomAsync ---

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_MachineMaster_Uses_UomId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineMasterWithUomAsync(uomId: 10);

            var result = await CreateRepo().HasLinkedUomAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedUomAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineMasterWithUomAsync(uomId: 20, isDeleted: true);

            var result = await CreateRepo().HasLinkedUomAsync(20);

            result.Should().BeFalse();
        }

        // --- HasActiveUomAsync ---

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_True_When_MachineMaster_Is_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineMasterWithUomAsync(uomId: 30, isActive: true);

            var result = await CreateRepo().HasActiveUomAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_MachineMaster_Is_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineMasterWithUomAsync(uomId: 40, isActive: false);

            var result = await CreateRepo().HasActiveUomAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMachineMasterWithUomAsync(uomId: 50, isDeleted: true);

            var result = await CreateRepo().HasActiveUomAsync(50);

            result.Should().BeFalse();
        }
    }
}
