using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MachineMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MachineMasterQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedMachineGroupAsync(string name = "MMQ_MG")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new MachineGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineGroup
                {
                    GroupName = name, Manufacturer = 1, UnitId = 1, DepartmentId = 1,
                    PowerSource = false,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private async Task<int> SeedShiftMasterAsync(string code = "MMQ_SM")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new ShiftMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ShiftMaster
                {
                    ShiftCode = code, ShiftName = $"Shift {code}",
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedCostCenterAsync(string code = "MMQ_CC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new CostCenterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.CostCenter
                {
                    CostCenterCode = code, CostCenterName = $"CC {code}",
                    UnitId = 1, DepartmentId = 1, EffectiveDate = DateTimeOffset.UtcNow,
                    ResponsiblePerson = "Test Person",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedWorkCenterAsync(string code = "MMQ_WC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new WorkCenterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.WorkCenter
                {
                    WorkCenterCode = code, WorkCenterName = $"WC {code}",
                    UnitId = 1, DepartmentId = 1,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedMiscMasterAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscTypeResult = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"MMQ_MT_{suffix}", Description = "Test Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var miscResult = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeResult.Id, Code = $"MMQ_MM_{suffix}", Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return miscResult.Id;
        }

        private async Task<int> SeedEntityAsync(string code, string name, int mgId, int smId, int ccId, int wcId, int lineNoId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MachineMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineMaster
                {
                    MachineCode = code, MachineName = name,
                    MachineGroupId = mgId, UnitId = 1,
                    ShiftMasterId = smId, CostCenterId = ccId, WorkCenterId = wcId,
                    UomId = 1, LineNo = lineNoId, AssetId = 0, IsProductionMachine = false,
                    InstallationDate = DateTimeOffset.UtcNow,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineGroup]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMasterDetails]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[CostCenter]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[WorkCenter]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMachineAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG1");
            var smId = await SeedShiftMasterAsync("MMQ_SM1");
            var ccId = await SeedCostCenterAsync("MMQ_CC1");
            var wcId = await SeedWorkCenterAsync("MMQ_WC1");
            var lineNoId = await SeedMiscMasterAsync("Q1");
            await SeedEntityAsync("MCH_QRY001", "Query Machine", mgId, smId, ccId, wcId, lineNoId);

            var items = await CreateQueryRepo().GetAllMachineAsync(null);

            items.Should().HaveCount(1);
            items[0].MachineCode.Should().Be("MCH_QRY001");
        }

        [Fact]
        public async Task GetAllMachineAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG2");
            var smId = await SeedShiftMasterAsync("MMQ_SM2");
            var ccId = await SeedCostCenterAsync("MMQ_CC2");
            var wcId = await SeedWorkCenterAsync("MMQ_WC2");
            var lineNoId = await SeedMiscMasterAsync("Q2");
            var id = await SeedEntityAsync("MCH_DEL1", "Delete Machine", mgId, smId, ccId, wcId, lineNoId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var items = await CreateQueryRepo().GetAllMachineAsync(null);

            items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllMachineAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG3");
            var smId = await SeedShiftMasterAsync("MMQ_SM3");
            var ccId = await SeedCostCenterAsync("MMQ_CC3");
            var wcId = await SeedWorkCenterAsync("MMQ_WC3");
            var lineNoId = await SeedMiscMasterAsync("Q3");
            await SeedEntityAsync("MCH_ALPHA", "Alpha Machine", mgId, smId, ccId, wcId, lineNoId);
            await SeedEntityAsync("MCH_BETA", "Beta Machine", mgId, smId, ccId, wcId, lineNoId);

            var items = await CreateQueryRepo().GetAllMachineAsync("Alpha");

            items.Should().HaveCount(1);
            items[0].MachineCode.Should().Be("MCH_ALPHA");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG4");
            var smId = await SeedShiftMasterAsync("MMQ_SM4");
            var ccId = await SeedCostCenterAsync("MMQ_CC4");
            var wcId = await SeedWorkCenterAsync("MMQ_WC4");
            var lineNoId = await SeedMiscMasterAsync("Q4");
            var id = await SeedEntityAsync("MCH_ID1", "GetById Machine", mgId, smId, ccId, wcId, lineNoId);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MachineCode.Should().Be("MCH_ID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG5");
            var smId = await SeedShiftMasterAsync("MMQ_SM5");
            var ccId = await SeedCostCenterAsync("MMQ_CC5");
            var wcId = await SeedWorkCenterAsync("MMQ_WC5");
            var lineNoId = await SeedMiscMasterAsync("Q5");
            var id = await SeedEntityAsync("MCH_SDEL", "Soft Del Machine", mgId, smId, ccId, wcId, lineNoId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG6");
            var smId = await SeedShiftMasterAsync("MMQ_SM6");
            var ccId = await SeedCostCenterAsync("MMQ_CC6");
            var wcId = await SeedWorkCenterAsync("MMQ_WC6");
            var lineNoId = await SeedMiscMasterAsync("Q6");
            var id = await SeedEntityAsync("MCH_NF1", "Exists Machine", mgId, smId, ccId, wcId, lineNoId);

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var mgId = await SeedMachineGroupAsync("MMQ_MG7");
            var smId = await SeedShiftMasterAsync("MMQ_SM7");
            var ccId = await SeedCostCenterAsync("MMQ_CC7");
            var wcId = await SeedWorkCenterAsync("MMQ_WC7");
            var lineNoId = await SeedMiscMasterAsync("Q7");
            var id = await SeedEntityAsync("MCH_NOLINK", "No Link Machine", mgId, smId, ccId, wcId, lineNoId);

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
