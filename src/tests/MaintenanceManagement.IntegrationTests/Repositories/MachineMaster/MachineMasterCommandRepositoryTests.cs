using Microsoft.EntityFrameworkCore;
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
    public sealed class MachineMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineMasterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMachineGroupAsync(string name = "MM_CMD_MG")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MachineGroupCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = name,
                Manufacturer = 1,
                UnitId = 1,
                DepartmentId = 1,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedShiftMasterAsync(string code = "MM_CMD_SM")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ShiftMasterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                ShiftCode = code,
                ShiftName = $"Shift {code}",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedCostCenterAsync(string code = "MM_CMD_CC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CostCenterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = code,
                CostCenterName = $"CC {code}",
                UnitId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedWorkCenterAsync(string code = "MM_CMD_WC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new WorkCenterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                WorkCenterCode = code,
                WorkCenterName = $"WC {code}",
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedMiscMasterAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscTypeResult = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"MM_MT_{suffix}", Description = "Test Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var miscResult = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeResult.Id, Code = $"MM_MM_{suffix}", Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return miscResult.Id;
        }

        private MaintenanceManagement.Domain.Entities.MachineMaster BuildEntity(
            string code,
            string name,
            int machineGroupId,
            int shiftMasterId,
            int costCenterId,
            int workCenterId,
            int lineNoId) =>
            new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                MachineCode = code,
                MachineName = name,
                MachineGroupId = machineGroupId,
                UnitId = 1,
                ShiftMasterId = shiftMasterId,
                CostCenterId = costCenterId,
                WorkCenterId = workCenterId,
                UomId = 1,
                LineNo = lineNoId,
                AssetId = 0,
                IsProductionMachine = false,
                InstallationDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG1");
            var smId = await SeedShiftMasterAsync("MM_CMD_S1");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC1");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC1");
            var lineNoId = await SeedMiscMasterAsync("C1");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH001", "Test Machine", mgId, smId, ccId, wcId, lineNoId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG2");
            var smId = await SeedShiftMasterAsync("MM_CMD_S2");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC2");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC2");
            var lineNoId = await SeedMiscMasterAsync("C2");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH002", "Lathe Machine", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MachineCode.Should().Be("MCH002");
            saved.MachineName.Should().Be("Lathe Machine");
            saved.MachineGroupId.Should().Be(mgId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG3");
            var smId = await SeedShiftMasterAsync("MM_CMD_S3");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC3");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC3");
            var lineNoId = await SeedMiscMasterAsync("C3");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH003", "CNC Machine", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG4");
            var smId = await SeedShiftMasterAsync("MM_CMD_S4");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC4");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC4");
            var lineNoId = await SeedMiscMasterAsync("C4");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH004", "Original Machine", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                MachineCode = "MCH004",
                MachineName = "Updated Machine",
                MachineGroupId = mgId,
                UnitId = 1,
                ShiftMasterId = smId,
                CostCenterId = ccId,
                WorkCenterId = wcId,
                UomId = 1,
                LineNo = lineNoId,
                AssetId = 0,
                IsProductionMachine = false,
                InstallationDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG5");
            var smId = await SeedShiftMasterAsync("MM_CMD_S5");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC5");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC5");
            var lineNoId = await SeedMiscMasterAsync("C5");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH005", "Before Update", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                MachineCode = "MCH005",
                MachineName = "After Update",
                MachineGroupId = mgId,
                UnitId = 1,
                ShiftMasterId = smId,
                CostCenterId = ccId,
                WorkCenterId = wcId,
                UomId = 1,
                LineNo = lineNoId,
                AssetId = 0,
                IsProductionMachine = false,
                InstallationDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MachineMaster.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.MachineName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                MachineCode = "NOEX",
                MachineName = "No Such Machine",
                MachineGroupId = 1,
                UnitId = 1,
                ShiftMasterId = 1,
                CostCenterId = 1,
                WorkCenterId = 1,
                UomId = 1,
                LineNo = 1,
                AssetId = 0,
                IsProductionMachine = false,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeFalse();
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG6");
            var smId = await SeedShiftMasterAsync("MM_CMD_S6");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC6");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC6");
            var lineNoId = await SeedMiscMasterAsync("C6");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH006", "Delete Machine", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mgId = await SeedMachineGroupAsync("MM_CMD_MG7");
            var smId = await SeedShiftMasterAsync("MM_CMD_S7");
            var ccId = await SeedCostCenterAsync("MM_CMD_CC7");
            var wcId = await SeedWorkCenterAsync("MM_CMD_WC7");
            var lineNoId = await SeedMiscMasterAsync("C7");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("MCH007", "Soft Delete Machine", mgId, smId, ccId, wcId, lineNoId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MachineMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.MachineMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
