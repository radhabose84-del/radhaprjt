using Dapper;
using Microsoft.Data.SqlClient;
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
    public sealed class GeneratorConsumptionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GeneratorConsumptionQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private GeneratorConsumptionQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new GeneratorConsumptionQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedMachineGroupAsync(string name, bool powerSource = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var res = await new MachineGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineGroup
                {
                    GroupName = name, Manufacturer = 1, UnitId = 1, DepartmentId = 1,
                    PowerSource = powerSource,
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
                    MiscTypeCode = $"GCQ_MT_{suffix}", Description = "Test",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = $"GCQ_MM_{suffix}", Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return misc.Id;
        }

        private async Task<int> SeedGeneratorAsync(string suffix, bool powerSource = true)
        {
            var mgId = await SeedMachineGroupAsync($"GCQ_MG_{suffix}", powerSource);
            var smId = await SeedShiftMasterAsync($"GCQ_SM_{suffix}");
            var ccId = await SeedCostCenterAsync($"GCQ_CC_{suffix}");
            var wcId = await SeedWorkCenterAsync($"GCQ_WC_{suffix}");
            var lineId = await SeedMiscMasterAsync($"LN_{suffix}");

            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MachineMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineMaster
                {
                    MachineCode = $"GEN_Q_{suffix}",
                    MachineName = $"Gen Machine {suffix}",
                    MachineGroupId = mgId, UnitId = 1,
                    ShiftMasterId = smId, CostCenterId = ccId, WorkCenterId = wcId,
                    UomId = 1, LineNo = lineId, AssetId = 0, IsProductionMachine = false,
                    ProductionCapacity = 1000m,
                    InstallationDate = DateTimeOffset.UtcNow,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedGeneratorConsumptionAsync(int generatorId, int purposeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new GeneratorConsumptionCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption
                {
                    GeneratorId = generatorId,
                    StartTime = DateTimeOffset.UtcNow.AddHours(-2),
                    EndTime = DateTimeOffset.UtcNow,
                    RunningHours = 2.0m,
                    DieselConsumption = 10.0m,
                    OpeningEnergyReading = 100.0m,
                    ClosingEnergyReading = 200.0m,
                    Energy = 100.0m,
                    UnitId = 1,
                    PurposeId = purposeId,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllGeneratorConsumptionAsync_Should_Return_Empty_When_No_Records()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllGeneratorConsumptionAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllGeneratorConsumptionAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var generatorId = await SeedGeneratorAsync("G1");
            var purposeId = await SeedMiscMasterAsync("PRP1");
            await SeedGeneratorConsumptionAsync(generatorId, purposeId);

            var (items, total) = await CreateQueryRepo().GetAllGeneratorConsumptionAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllGeneratorConsumptionAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var generatorId = await SeedGeneratorAsync("G2");
            var purposeId = await SeedMiscMasterAsync("PRP2");
            var gcId = await SeedGeneratorConsumptionAsync(generatorId, purposeId);

            // Soft-delete the consumption row via raw SQL
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                "UPDATE [Maintenance].[GeneratorConsumption] SET IsDeleted = 1 WHERE Id = @Id",
                new { Id = gcId });

            var (items, total) = await CreateQueryRepo().GetAllGeneratorConsumptionAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET MACHINES BASED ON UNIT ---

        [Fact]
        public async Task GetMachineIdBasedonUnit_Should_Return_Empty_When_No_Power_Machines()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMachineIdBasedonUnit();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMachineIdBasedonUnit_Should_Return_PowerSource_Machines()
        {
            await ClearTablesAsync();
            await SeedGeneratorAsync("PM1", powerSource: true);

            var result = await CreateQueryRepo().GetMachineIdBasedonUnit();

            result.Should().HaveCount(1);
            result[0].MachineCode.Should().Be("GEN_Q_PM1");
        }

        // --- OPENING READER VALUE ---

        [Fact]
        public async Task GetOpeningReaderValueById_Should_Throw_When_Generator_NotFound()
        {
            await ClearTablesAsync();

            Func<Task> act = async () => await CreateQueryRepo().GetOpeningReaderValueById(99999);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task GetOpeningReaderValueById_Should_Return_Reader_For_Valid_Generator()
        {
            await ClearTablesAsync();
            var generatorId = await SeedGeneratorAsync("RD1");

            var result = await CreateQueryRepo().GetOpeningReaderValueById(generatorId);

            result.Should().NotBeNull();
            result.GeneratorId.Should().Be(generatorId);
        }
    }
}
