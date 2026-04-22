using Dapper;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for PartyMasterMaintenanceValidationRepository.
    /// Validates HasLinkedPartyMasterAsync and HasActivePartyMasterAsync against
    /// MaintenanceRequest.VendorId column.
    /// MaintenanceRequest has many FK dependencies, so we use raw SQL with NOCHECK
    /// constraints for minimal seeding.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterMaintenanceValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterMaintenanceValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterMaintenanceValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterMaintenanceValidationRepository(conn);
        }

        /// <summary>
        /// Seeds a minimal MaintenanceRequest row via raw SQL with FK constraints temporarily disabled.
        /// MaintenanceRequest references MachineId (FK to MachineMaster), RequestTypeId, MaintenanceTypeId (FK to MiscMaster).
        /// </summary>
        private async Task SeedMaintenanceRequestWithVendorAsync(
            int vendorId,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[MaintenanceRequest] NOCHECK CONSTRAINT ALL;");

            await conn.ExecuteAsync(@"
                INSERT INTO [Maintenance].[MaintenanceRequest]
                    (RequestTypeId, MaintenanceTypeId, MachineId, CompanyId, UnitId,
                     MaintenanceDepartmentId, ProductionDepartmentId, SourceId,
                     VendorId, IsActive, IsDeleted,
                     CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                VALUES
                    (1, 1, 1, 1, 1,
                     1, 1, 1,
                     @VendorId, @IsActive, @IsDeleted,
                     1, 'test-user', '127.0.0.1', GETUTCDATE());",
                new
                {
                    VendorId = vendorId,
                    IsActive = isActive,
                    IsDeleted = isDeleted
                });

            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[MaintenanceRequest] CHECK CONSTRAINT ALL;");
        }

        // --- HasLinkedPartyMasterAsync ---

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_True_When_Request_Uses_VendorId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMaintenanceRequestWithVendorAsync(vendorId: 10);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedPartyMasterAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMaintenanceRequestWithVendorAsync(vendorId: 20, isDeleted: true);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(20);

            result.Should().BeFalse();
        }

        // --- HasActivePartyMasterAsync ---

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_True_When_Request_Is_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMaintenanceRequestWithVendorAsync(vendorId: 30, isActive: true);

            var result = await CreateRepo().HasActivePartyMasterAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_False_When_Request_Is_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMaintenanceRequestWithVendorAsync(vendorId: 40, isActive: false);

            var result = await CreateRepo().HasActivePartyMasterAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedMaintenanceRequestWithVendorAsync(vendorId: 50, isDeleted: true);

            var result = await CreateRepo().HasActivePartyMasterAsync(50);

            result.Should().BeFalse();
        }
    }
}
