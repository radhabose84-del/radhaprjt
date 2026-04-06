using Microsoft.EntityFrameworkCore;
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
    public sealed class MachineSpecificationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineSpecificationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineSpecificationCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<(int machineId, int miscMasterId)> SeedPrerequisitesAsync(string suffix)
        {
            // Seed MiscTypeMaster
            var miscTypeId = await SeedMiscTypeMasterAsync($"MSP_MT_{suffix}");

            // Seed MiscMaster (for SpecificationId)
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, $"MSP_MM_{suffix}");

            // Seed FK parents for MachineMaster
            var mgId = await SeedMachineGroupAsync($"MSP_MG_{suffix}");
            var smId = await SeedShiftMasterAsync($"MSP_SM_{suffix}");
            var ccId = await SeedCostCenterAsync($"MSP_CC_{suffix}");
            var wcId = await SeedWorkCenterAsync($"MSP_WC_{suffix}");

            // Seed MachineMaster (reuse miscMasterId for LineNo FK)
            var machineId = await SeedMachineMasterAsync($"MSP_M_{suffix}", mgId, smId, ccId, wcId, miscMasterId);

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

        private async Task ClearAllTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MiscTypeMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineGroup]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[ShiftMasterDetails]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[ShiftMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[CostCenter]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[WorkCenter]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("C1");

            var newId = await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId,
                    SpecificationId = miscMasterId,
                    SpecificationValue = "100 RPM",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("C2");

            var newId = await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId,
                    SpecificationId = miscMasterId,
                    SpecificationValue = "200 RPM",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineSpecification.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MachineId.Should().Be(machineId);
            saved.SpecificationId.Should().Be(miscMasterId);
            saved.SpecificationValue.Should().Be("200 RPM");
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("C3");

            var newId = await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId,
                    SpecificationId = miscMasterId,
                    SpecificationValue = "300 RPM",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineSpecification.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE (replaces all records for machineId) ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("U1");

            await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "Old Value",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>
            {
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "New Value",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                }
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Old_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("U2");

            await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "Original",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>
            {
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "Replaced",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                }
            });
            ctx.ChangeTracker.Clear();

            var records = ctx.MachineSpecification.Where(x => x.MachineId == machineId).ToList();
            records.Should().HaveCount(1);
            records[0].SpecificationValue.Should().Be("Replaced");
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("D1");

            var newId = await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "To Delete",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineSpecification { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);
            var (machineId, miscMasterId) = await SeedPrerequisitesAsync("D2");

            var newId = await CreateRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MachineSpecification
                {
                    MachineId = machineId, SpecificationId = miscMasterId,
                    SpecificationValue = "Soft Delete Value",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineSpecification { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MachineSpecification
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_MinusOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAllTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.MachineSpecification { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }
    }
}
