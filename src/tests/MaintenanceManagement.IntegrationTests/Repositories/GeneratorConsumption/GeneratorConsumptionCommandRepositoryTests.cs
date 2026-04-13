using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.Power.GeneratorConsumption;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.GeneratorConsumption
{
    [Collection("DatabaseCollection")]
    public sealed class GeneratorConsumptionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GeneratorConsumptionCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private GeneratorConsumptionCommandRepository CreateRepo(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMachineGroupAsync(string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var res = await new MachineGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineGroup
                {
                    GroupName = name, Manufacturer = 1, UnitId = 1, DepartmentId = 1,
                    PowerSource = true,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return res.Id;
        }

        private async Task<int> SeedShiftMasterAsync(string code)
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

        private async Task<int> SeedCostCenterAsync(string code)
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

        private async Task<int> SeedWorkCenterAsync(string code)
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
            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"GC_MT_{suffix}", Description = "Test Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = $"GC_MM_{suffix}", Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return misc.Id;
        }

        private async Task<int> SeedGeneratorMachineAsync(string suffix)
        {
            var mgId = await SeedMachineGroupAsync($"GC_MG_{suffix}");
            var smId = await SeedShiftMasterAsync($"GC_SM_{suffix}");
            var ccId = await SeedCostCenterAsync($"GC_CC_{suffix}");
            var wcId = await SeedWorkCenterAsync($"GC_WC_{suffix}");
            var lineId = await SeedMiscMasterAsync($"LINE_{suffix}");

            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MachineMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineMaster
                {
                    MachineCode = $"GEN_{suffix}",
                    MachineName = $"Generator {suffix}",
                    MachineGroupId = mgId, UnitId = 1,
                    ShiftMasterId = smId, CostCenterId = ccId, WorkCenterId = wcId,
                    UomId = 1, LineNo = lineId, AssetId = 0, IsProductionMachine = false,
                    InstallationDate = DateTimeOffset.UtcNow,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private static MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption BuildEntity(
            int generatorId, int? purposeId = null) =>
            new MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption
            {
                GeneratorId = generatorId,
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow,
                RunningHours = 1.0m,
                DieselConsumption = 5.0m,
                OpeningEnergyReading = 100.0m,
                ClosingEnergyReading = 150.0m,
                Energy = 50.0m,
                UnitId = 1,
                PurposeId = purposeId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var generatorId = await SeedGeneratorMachineAsync("C1");
            var purposeId = await SeedMiscMasterAsync("P1");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(generatorId, purposeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var generatorId = await SeedGeneratorMachineAsync("C2");
            var purposeId = await SeedMiscMasterAsync("P2");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(generatorId, purposeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.GeneratorConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.GeneratorId.Should().Be(generatorId);
            saved.PurposeId.Should().Be(purposeId);
            saved.RunningHours.Should().Be(1.0m);
            saved.DieselConsumption.Should().Be(5.0m);
            saved.Energy.Should().Be(50.0m);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var generatorId = await SeedGeneratorMachineAsync("C3");
            var purposeId = await SeedMiscMasterAsync("P3");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(generatorId, purposeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.GeneratorConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Multiple_Records_Should_Get_Sequential_Ids()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var generatorId = await SeedGeneratorMachineAsync("C4");
            var purposeId = await SeedMiscMasterAsync("P4");

            var id1 = await CreateRepo(ctx).CreateAsync(BuildEntity(generatorId, purposeId));
            var id2 = await CreateRepo(ctx).CreateAsync(BuildEntity(generatorId, purposeId));

            id1.Should().BeGreaterThan(0);
            id2.Should().BeGreaterThan(id1);
        }
    }
}
