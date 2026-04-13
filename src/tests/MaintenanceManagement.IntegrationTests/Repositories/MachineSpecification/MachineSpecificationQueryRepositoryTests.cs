using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineMaster;
using MaintenanceManagement.Infrastructure.Repositories.MachineSpecification;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineSpecification
{
    [Collection("DatabaseCollection")]
    public sealed class MachineSpecificationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineSpecificationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineSpecificationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MachineSpecificationQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<(int machineId, int miscMasterId)> SeedPrerequisitesAsync(string suffix)
        {
            var miscTypeId = await SeedMiscTypeMasterAsync($"MSPQ_MT_{suffix}");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, $"MSPQ_MM_{suffix}");
            var mgId = await SeedMachineGroupAsync($"MSPQ_MG_{suffix}");
            var smId = await SeedShiftMasterAsync($"MSPQ_SM_{suffix}");
            var ccId = await SeedCostCenterAsync($"MSPQ_CC_{suffix}");
            var wcId = await SeedWorkCenterAsync($"MSPQ_WC_{suffix}");
            var machineId = await SeedMachineMasterAsync($"MSPQ_M_{suffix}", mgId, smId, ccId, wcId, miscMasterId);
            return (machineId, miscMasterId);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code, Description = "Test Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = $"Desc {code}",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private async Task<int> SeedMachineGroupAsync(string name)
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

        private async Task<int> SeedMachineMasterAsync(string code, int mgId, int smId, int ccId, int wcId, int lineNoId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MachineMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineMaster
                {
                    MachineCode = code, MachineName = $"Machine {code}",
                    MachineGroupId = mgId, UnitId = 1,
                    ShiftMasterId = smId, CostCenterId = ccId, WorkCenterId = wcId,
                    UomId = 1, LineNo = lineNoId, AssetId = 0, IsProductionMachine = false,
                    InstallationDate = DateTimeOffset.UtcNow,
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedSpecificationAsync(int machineId, int specificationId, string value = "Test Value")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MachineSpecificationCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId,
                    SpecificationId = specificationId,
                    SpecificationValue = value,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearAllTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET BY ID (returns list for machineId) ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Specifications_For_Machine()
        {
            await ClearAllTablesAsync();
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("Q1");
            await SeedSpecificationAsync(machineId, miscMasterId, "500 KG");

            var results = await CreateQueryRepo().GetByIdAsync(machineId);

            results.Should().HaveCount(1);
            results[0].SpecificationValue.Should().Be("500 KG");
            results[0].MachineId.Should().Be(machineId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_When_No_Specs()
        {
            await ClearAllTablesAsync();
            var (machineId, _) = await SeedPrerequisitesAsync("Q2");

            var results = await CreateQueryRepo().GetByIdAsync(machineId);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAllTablesAsync();
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("Q3");
            var specId = await SeedSpecificationAsync(machineId, miscMasterId, "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineSpecificationCommandRepository(ctx).DeleteAsync(specId,
                new MaintenanceManagement.Domain.Entities.MachineSpecification { IsDeleted = BaseEntity.IsDelete.Deleted });

            var results = await CreateQueryRepo().GetByIdAsync(machineId);

            results.Should().BeEmpty();
        }

        // --- GET BY SPECIFICATION ID ---

        [Fact]
        public async Task GetBySpecificationIdAsync_Should_Return_Correct_Dto()
        {
            await ClearAllTablesAsync();
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("Q4");
            var specId = await SeedSpecificationAsync(machineId, miscMasterId, "Spec Value Q4");

            var result = await CreateQueryRepo().GetBySpecificationIdAsync(specId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(specId);
        }

        [Fact]
        public async Task GetBySpecificationIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAllTablesAsync();

            var result = await CreateQueryRepo().GetBySpecificationIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBySpecificationIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAllTablesAsync();
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("Q5");
            var specId = await SeedSpecificationAsync(machineId, miscMasterId, "Soft Del Spec");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineSpecificationCommandRepository(ctx).DeleteAsync(specId,
                new MaintenanceManagement.Domain.Entities.MachineSpecification { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetBySpecificationIdAsync(specId);

            result.Should().BeNull();
        }
    }
}
