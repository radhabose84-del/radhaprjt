using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceRequestQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetMaintenancestatusAsync ---

        [Fact]
        public async Task GetMaintenancestatusAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenancestatusAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceOpenstatusAsync ---

        [Fact]
        public async Task GetMaintenanceOpenstatusAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceOpenstatusAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceRequestTypeAsync ---

        [Fact]
        public async Task GetMaintenanceRequestTypeAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceRequestTypeAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceStatusDescAsync ---

        [Fact]
        public async Task GetMaintenanceStatusDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceStatusDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetWOclosedAsync ---

        [Fact]
        public async Task GetWOclosedAsync_Should_Return_False_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWOclosedAsync(999999);

            result.Should().BeFalse();
        }

        // --- GetWOclosedOrInProgressAsync ---

        [Fact]
        public async Task GetWOclosedOrInProgressAsync_Should_Return_False_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWOclosedOrInProgressAsync(999999);

            result.Should().BeFalse();
        }

        // --- GetMachineInfoAsync ---

        [Fact]
        public async Task GetMachineInfoAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMachineInfoAsync(999999);

            // Dapper ValueTuple default: returns default (null, 0, 0) when nothing — result is (null,0,0)
            // The return type is Nullable<tuple> so null is possible.
            // We accept either null or default-valued tuple.
            (result == null || result.Value.Id == 0).Should().BeTrue();
        }

        // --- HasActiveRequestForMachineAsync (SCRUM-1475 — duplicate-machine guard) ---

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Return_True_When_Open_Request_Exists()
        {
            await ClearTablesAsync();
            var (openId, _, _) = await SeedStatusMastersAsync();
            await SeedMaintenanceRequestRawAsync(machineId: 100, requestStatusId: openId);

            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(100);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Return_True_When_InProgress_Request_Exists()
        {
            await ClearTablesAsync();
            var (_, inProgressId, _) = await SeedStatusMastersAsync();
            await SeedMaintenanceRequestRawAsync(machineId: 101, requestStatusId: inProgressId);

            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(101);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Return_False_When_Only_Closed_Request_Exists()
        {
            await ClearTablesAsync();
            var (_, _, closedId) = await SeedStatusMastersAsync();
            await SeedMaintenanceRequestRawAsync(machineId: 102, requestStatusId: closedId);

            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(102);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Return_False_When_Request_IsDeleted()
        {
            await ClearTablesAsync();
            var (openId, _, _) = await SeedStatusMastersAsync();
            await SeedMaintenanceRequestRawAsync(machineId: 103, requestStatusId: openId, isDeleted: 1);

            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(103);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Ignore_Excluded_RequestId()
        {
            await ClearTablesAsync();
            var (openId, _, _) = await SeedStatusMastersAsync();
            var requestId = await SeedMaintenanceRequestRawAsync(machineId: 104, requestStatusId: openId);

            // Excluding the only active row → no other active rows for this machine
            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(104, excludeRequestId: requestId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveRequestForMachineAsync_Should_Return_False_For_Different_Machine()
        {
            await ClearTablesAsync();
            var (openId, _, _) = await SeedStatusMastersAsync();
            await SeedMaintenanceRequestRawAsync(machineId: 105, requestStatusId: openId);

            // Active request exists for machine 105, querying for 999 → no match
            var result = await CreateQueryRepo().HasActiveRequestForMachineAsync(999);

            result.Should().BeFalse();
        }

        // --- Seed helpers for HasActiveRequestForMachineAsync ---

        // Seeds the 'Status' MiscType + Open / InProgress / Closed MiscMaster rows (matching MiscEnumEntity codes).
        // Returns the Ids of (Open, InProgress, Closed).
        private async Task<(int openId, int inProgressId, int closedId)> SeedStatusMastersAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var miscTypeId = (await typeRepo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = MiscEnumEntity.WOStatus.MiscCode, // "Status"
                Description = "WorkOrder Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            var miscRepo = new MiscMasterCommandRepository(ctx);

            int openId = (await miscRepo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = MiscEnumEntity.StatusOpen.Code,            // "Open"
                Description = "Open",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            int inProgressId = (await miscRepo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = MiscEnumEntity.GetStatusId.Status,         // "InProgress"
                Description = "InProgress",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            int closedId = (await miscRepo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = MiscEnumEntity.MaintenanceStatusUpdate.Code, // "Closed"
                Description = "Closed",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (openId, inProgressId, closedId);
        }

        // Inserts a MaintenanceRequest row directly via SQL with FK constraints disabled, so we don't have
        // to seed the entire MachineMaster → MachineGroup → ShiftMaster → … chain just for a status query test.
        // The query under test only filters by MachineId / RequestStatusId / IsDeleted — no JOIN to MachineMaster.
        private async Task<int> SeedMaintenanceRequestRawAsync(int machineId, int requestStatusId, byte isDeleted = 0)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Disable FK checks on this table so MachineId / department FKs aren't enforced for test seed
            await conn.ExecuteAsync(
                "ALTER TABLE Maintenance.MaintenanceRequest NOCHECK CONSTRAINT ALL;");

            const string insertSql = @"
                INSERT INTO Maintenance.MaintenanceRequest
                    (RequestTypeId, MaintenanceTypeId, MachineId, CompanyId, UnitId,
                     MaintenanceDepartmentId, ProductionDepartmentId, SourceId,
                     RequestStatusId, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES (1, 1, @MachineId, 1, 1,
                        1, 1, 0,
                        @RequestStatusId, 1, @IsDeleted,
                        1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');";

            var newId = await conn.ExecuteScalarAsync<int>(insertSql, new
            {
                MachineId = machineId,
                RequestStatusId = requestStatusId,
                IsDeleted = isDeleted
            });

            await conn.ExecuteAsync(
                "ALTER TABLE Maintenance.MaintenanceRequest WITH CHECK CHECK CONSTRAINT ALL;");

            return newId;
        }
    }
}
