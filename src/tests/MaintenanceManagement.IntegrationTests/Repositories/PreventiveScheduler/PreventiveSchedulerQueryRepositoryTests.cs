using Dapper;
using MaintenanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using Contracts.Interfaces;

namespace MaintenanceManagement.IntegrationTests.Repositories.PreventiveScheduler
{
    [Collection("DatabaseCollection")]
    public sealed class PreventiveSchedulerQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PreventiveSchedulerQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers.PreventiveSchedulerQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new(conn, _fixture.IpMock.Object);
        }

        /// <summary>
        /// Seeds a MachineGroup + MiscMaster (for FK refs) + PreventiveSchedulerHeader
        /// </summary>
        private async Task<int> SeedPreventiveSchedulerAsync(string name = "Monthly PM")
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Ensure FK dependencies
            var machineGroup = ctx.Set<MaintenanceManagement.Domain.Entities.MachineGroup>().FirstOrDefault() ?? new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = "Test Group",
                Manufacturer = 1,
                UnitId = 1,
                DepartmentId = 1,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            if (machineGroup.Id == 0)
            {
                ctx.Set<MaintenanceManagement.Domain.Entities.MachineGroup>().Add(machineGroup);
                await ctx.SaveChangesAsync();
            }

            // Seed MiscMasters (maintenance category, schedule, frequency type, frequency unit)
            var miscIds = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                var existing = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FirstOrDefaultAsync(m => m.Code == $"PM{i}");
                if (existing != null)
                {
                    miscIds.Add(existing.Id);
                }
                else
                {
                    var misc = new MaintenanceManagement.Domain.Entities.MiscMaster
                    {
                        MiscTypeId = 1,
                        Code = $"PM{i}",
                        Description = $"Test MiscMaster {i}",
                        SortOrder = i,
                        IsActive = BaseEntity.Status.Active,
                        IsDeleted = BaseEntity.IsDelete.NotDeleted
                    };
                    ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().Add(misc);
                    await ctx.SaveChangesAsync();
                    miscIds.Add(misc.Id);
                }
            }

            var header = new PreventiveSchedulerHeader
            {
                PreventiveSchedulerName = name,
                MachineGroupId = machineGroup.Id,
                MachineGroup = machineGroup,
                DepartmentId = 1,
                MaintenanceCategoryId = miscIds[0],
                MiscMaintenanceCategory = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FindAsync(miscIds[0])!,
                ScheduleId = miscIds[1],
                MiscSchedule = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FindAsync(miscIds[1])!,
                FrequencyTypeId = miscIds[2],
                MiscFrequencyType = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FindAsync(miscIds[2])!,
                FrequencyInterval = 30,
                FrequencyUnitId = miscIds[3],
                MiscFrequencyUnit = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FindAsync(miscIds[3])!,
                EffectiveDate = new DateOnly(2025, 1, 1),
                GraceDays = 3,
                ReminderWorkOrderDays = 5,
                ReminderMaterialReqDays = 7,
                IsDownTimeRequired = 1,
                DownTimeEstimateHrs = 4.5m,
                UnitId = 1,
                CompanyId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.Set<MaintenanceManagement.Domain.Entities.PreventiveSchedulerHeader>().Add(header);
            await ctx.SaveChangesAsync();
            return header.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[PreventiveSchedulerItems]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[PreventiveSchedulerActivity]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[PreventiveSchedulerDetail]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[PreventiveSchedulerHeader]");
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_ExistingRecord_ReturnsTrue()
        {
            await ClearTablesAsync();
            var id = await SeedPreventiveSchedulerAsync();

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_NonExistent_ReturnsFalse()
        {
            var result = await CreateQueryRepo().NotFoundAsync(999999);

            result.Should().BeFalse();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_ExistingRecord_ReturnsEntity()
        {
            await ClearTablesAsync();
            var id = await SeedPreventiveSchedulerAsync("Monthly PM Test");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_NonExistent_ReturnsFalse()
        {
            var result = await CreateQueryRepo().AlreadyExistsAsync(999, 999);

            result.Should().BeFalse();
        }
    }
}
