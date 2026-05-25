using Dapper;
using MaintenanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest.Esr
{
    /// <summary>
    /// Seeds the minimum MiscType + MiscMaster + MaintenanceRequest rows needed for
    /// the ESR linkage repository tests (lookup, validation, conversion).
    /// </summary>
    internal sealed class EsrTestSeedHelper
    {
        private readonly DbFixture _fixture;

        public EsrTestSeedHelper(DbFixture fixture) => _fixture = fixture;

        // MiscMaster ids populated after SeedStatusesAndTypesAsync runs
        public int RequestTypeMiscTypeId { get; private set; }
        public int StatusMiscTypeId { get; private set; }

        public int RequestTypeExternalId { get; private set; }
        public int RequestTypeInternalId { get; private set; }

        public int StatusOpenId { get; private set; }
        public int StatusInProgressId { get; private set; }
        public int StatusClosedId { get; private set; }
        public int StatusPartiallyConvertedId { get; private set; }
        public int StatusFullyConvertedId { get; private set; }

        /// <summary>Seeds the MiscTypeMaster + MiscMaster rows used by the ESR repos.</summary>
        public async Task SeedStatusesAndTypesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            RequestTypeMiscTypeId = await InsertMiscTypeAsync(conn, "RequestType", "Request Type");
            StatusMiscTypeId = await InsertMiscTypeAsync(conn, "MaintenanceStatus", "Maintenance Status");

            RequestTypeExternalId = await InsertMiscMasterAsync(conn, RequestTypeMiscTypeId, "External", "External");
            RequestTypeInternalId = await InsertMiscMasterAsync(conn, RequestTypeMiscTypeId, "Internal", "Internal");

            StatusOpenId = await InsertMiscMasterAsync(conn, StatusMiscTypeId, "Open", "Open");
            StatusInProgressId = await InsertMiscMasterAsync(conn, StatusMiscTypeId, "InProgress", "InProgress");
            StatusClosedId = await InsertMiscMasterAsync(conn, StatusMiscTypeId, "Closed", "Closed");
            StatusPartiallyConvertedId = await InsertMiscMasterAsync(conn, StatusMiscTypeId, "PartiallyConverted", "Partially Converted");
            StatusFullyConvertedId = await InsertMiscMasterAsync(conn, StatusMiscTypeId, "FullyConverted", "Fully Converted");
        }

        public async Task<int> SeedMaintenanceRequestAsync(
            int requestTypeId,
            int statusId,
            int vendorId = 7,
            int? serviceTypeId = null,
            decimal estimatedServiceCost = 100000m,
            decimal convertedToPoAmount = 0m,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // FK references on MaintenanceRequest (MachineMaster, etc.) are out of scope for ESR tests.
            // Temporarily disable FKs on this table so we can insert a minimal row.
            // ClearAllTablesAsync() will reset everything before the next test.
            await conn.ExecuteAsync(@"
                ALTER TABLE Maintenance.MaintenanceRequest NOCHECK CONSTRAINT ALL;");

            const string sql = @"
                INSERT INTO Maintenance.MaintenanceRequest
                  (RequestTypeId, MaintenanceTypeId, MachineId, CompanyId, UnitId,
                   MaintenanceDepartmentId, ProductionDepartmentId, SourceId,
                   VendorId, VendorName, ServiceTypeId, EstimatedServiceCost,
                   EstimatedSpareCost, ConvertedToPoAmount, Remarks,
                   RequestStatusId, IsActive, IsDeleted,
                   CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                  (@RequestTypeId, 1, 1, 1, 1,
                   1, 1, 1,
                   @VendorId, 'Test Vendor', @ServiceTypeId, @EstimatedServiceCost,
                   0, @ConvertedToPoAmount, 'Test request',
                   @StatusId, @IsActive, @IsDeleted,
                   1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                RequestTypeId = requestTypeId,
                StatusId = statusId,
                VendorId = vendorId,
                ServiceTypeId = serviceTypeId,
                EstimatedServiceCost = estimatedServiceCost,
                ConvertedToPoAmount = convertedToPoAmount,
                IsActive = isActive,
                IsDeleted = isDeleted
            });
        }

        public async Task<(int convertedAmount, int statusId)> GetRequestStateAsync(int requestId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            const string sql = @"
                SELECT ConvertedToPoAmount, RequestStatusId
                FROM Maintenance.MaintenanceRequest
                WHERE Id = @Id;";
            var row = await conn.QueryFirstAsync<(decimal converted, int status)>(sql, new { Id = requestId });
            return ((int)row.converted, row.status);
        }

        // --- private helpers ---

        private static async Task<int> InsertMiscTypeAsync(SqlConnection conn, string code, string description)
        {
            const string sql = @"
                INSERT INTO Maintenance.MiscTypeMaster
                  (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                  (@Code, @Description, 1, 0, 1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');";
            return await conn.ExecuteScalarAsync<int>(sql, new { Code = code, Description = description });
        }

        private static async Task<int> InsertMiscMasterAsync(SqlConnection conn, int miscTypeId, string code, string description)
        {
            const string sql = @"
                INSERT INTO Maintenance.MiscMaster
                  (MiscTypeId, Code, description, sortOrder, IsActive, IsDeleted,
                   CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                  (@MiscTypeId, @Code, @Description, 1, 1, 0,
                   1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');";
            return await conn.ExecuteScalarAsync<int>(sql, new { MiscTypeId = miscTypeId, Code = code, Description = description });
        }
    }
}
