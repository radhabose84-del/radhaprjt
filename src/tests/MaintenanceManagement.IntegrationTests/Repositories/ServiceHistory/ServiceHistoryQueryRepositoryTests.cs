using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.ServiceHistory;

namespace MaintenanceManagement.IntegrationTests.Repositories.ServiceHistory
{
    [Collection("DatabaseCollection")]
    public sealed class ServiceHistoryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ServiceHistoryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ServiceHistoryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ServiceHistoryQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task DisableMaintenanceConstraintsAsync(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ALL;'
                FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Maintenance';
                EXEC sp_executesql @sql;");
        }

        private async Task<int> SeedMachineAsync(SqlConnection conn, int assetId, string code = "MC-1")
        {
            const string sql = @"
                INSERT INTO Maintenance.MachineMaster
                    (MachineCode, MachineName, MachineGroupId, UnitId, UomId, ShiftMasterId,
                     CostCenterId, WorkCenterId, InstallationDate, AssetId, ProductionCapacity,
                     IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, [LineNo], IsProductionMachine)
                OUTPUT INSERTED.Id
                VALUES
                    (@Code, 'Test Machine', 1, 1, 1, 1,
                     1, 1, SYSDATETIMEOFFSET(), @AssetId, 0,
                     1, 0, 1, 'test-user', '127.0.0.1', 1, 0);";
            return await conn.ExecuteScalarAsync<int>(sql, new { Code = code, AssetId = assetId });
        }

        private async Task<int> SeedMaintenanceRequestAsync(SqlConnection conn, int machineId, int isDeleted = 0)
        {
            const string sql = @"
                INSERT INTO Maintenance.MaintenanceRequest
                    (RequestTypeId, MaintenanceTypeId, MachineId, CompanyId, UnitId,
                     MaintenanceDepartmentId, ProductionDepartmentId, SourceId, ConvertedToPoAmount,
                     Remarks, IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                OUTPUT INSERTED.Id
                VALUES
                    (1, 1, @MachineId, 1, 1,
                     1, 1, 1, 0,
                     'Bearing replacement', 1, @IsDeleted, 1, 'test-user', '127.0.0.1', SYSDATETIMEOFFSET());";
            return await conn.ExecuteScalarAsync<int>(sql, new { MachineId = machineId, IsDeleted = isDeleted });
        }

        [Fact]
        public async Task GetServiceHistory_NoData_ReturnsEmptyAndZero()
        {
            await _fixture.ClearAllTablesAsync();

            var (items, total) = await CreateQueryRepo()
                .GetServiceHistoryAsync(machineId: 999, assetId: null, fromDate: null, toDate: null,
                    pageNumber: 1, pageSize: 10);

            items.Should().NotBeNull();
            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetServiceHistory_ByMachineId_ReturnsMaintenanceRequestRow()
        {
            await _fixture.ClearAllTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await DisableMaintenanceConstraintsAsync(conn);

            var machineId = await SeedMachineAsync(conn, assetId: 42);
            await SeedMaintenanceRequestAsync(conn, machineId);

            var (items, total) = await CreateQueryRepo()
                .GetServiceHistoryAsync(machineId, null, null, null, 1, 10);

            total.Should().Be(1);
            items.Should().ContainSingle();
            items[0].EventType.Should().Be("MaintenanceRequest");
            items[0].MachineId.Should().Be(machineId);
            items[0].AssetId.Should().Be(42);
            items[0].Description.Should().Be("Bearing replacement");
        }

        [Fact]
        public async Task GetServiceHistory_ByAssetId_ResolvesThroughMachine()
        {
            await _fixture.ClearAllTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await DisableMaintenanceConstraintsAsync(conn);

            var machineId = await SeedMachineAsync(conn, assetId: 777);
            await SeedMaintenanceRequestAsync(conn, machineId);

            var (items, total) = await CreateQueryRepo()
                .GetServiceHistoryAsync(null, 777, null, null, 1, 10);

            total.Should().Be(1);
            items[0].AssetId.Should().Be(777);
        }

        [Fact]
        public async Task GetServiceHistory_ExcludesSoftDeletedRequest()
        {
            await _fixture.ClearAllTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await DisableMaintenanceConstraintsAsync(conn);

            var machineId = await SeedMachineAsync(conn, assetId: 5);
            await SeedMaintenanceRequestAsync(conn, machineId, isDeleted: 1);

            var (items, total) = await CreateQueryRepo()
                .GetServiceHistoryAsync(machineId, null, null, null, 1, 10);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }
    }
}
