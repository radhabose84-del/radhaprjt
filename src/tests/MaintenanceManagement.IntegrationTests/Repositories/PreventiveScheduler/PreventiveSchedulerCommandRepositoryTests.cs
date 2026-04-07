using Contracts.Interfaces;
using Dapper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Data;
using MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers;
using MaintenanceManagement.IntegrationTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.IntegrationTests.Repositories.PreventiveScheduler
{
    [Collection("DatabaseCollection")]
    public sealed class PreventiveSchedulerCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PreventiveSchedulerCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PreventiveSchedulerCommandRepository CreateRepository(
            ApplicationDbContext ctx,
            IPreventiveSchedulerQuery queryRepo = null)
        {
            var mockQueryRepo = new Mock<IPreventiveSchedulerQuery>(MockBehavior.Loose);
            var mockMiscRepo = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            var mockLogService = new Mock<IPreventiveScheduleLogService>(MockBehavior.Loose);
            var mockHttpContext = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            var mockLogger = new Mock<ILogger<PreventiveSchedulerCommandRepository>>(MockBehavior.Loose);

            return new PreventiveSchedulerCommandRepository(
                ctx,
                queryRepo ?? mockQueryRepo.Object,
                mockMiscRepo.Object,
                _fixture.IpMock.Object,
                mockLogService.Object,
                mockHttpContext.Object,
                mockLogger.Object);
        }

        private async Task<(int machineGroupId, int[] miscIds)> EnsureFKDependenciesAsync(ApplicationDbContext ctx)
        {
            var machineGroup = await ctx.Set<MaintenanceManagement.Domain.Entities.MachineGroup>().FirstOrDefaultAsync() ?? new MaintenanceManagement.Domain.Entities.MachineGroup
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

            var miscIds = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                var existing = await ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().FirstOrDefaultAsync(m => m.Code == $"CMD{i}");
                if (existing != null)
                {
                    miscIds.Add(existing.Id);
                }
                else
                {
                    var misc = new MaintenanceManagement.Domain.Entities.MiscMaster
                    {
                        MiscTypeId = 1, Code = $"CMD{i}", Description = $"Misc {i}", SortOrder = i,
                        IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                    };
                    ctx.Set<MaintenanceManagement.Domain.Entities.MiscMaster>().Add(misc);
                    await ctx.SaveChangesAsync();
                    miscIds.Add(misc.Id);
                }
            }
            return (machineGroup.Id, miscIds.ToArray());
        }

        private PreventiveSchedulerHeader BuildEntity(int machineGroupId, int[] miscIds) =>
            new PreventiveSchedulerHeader
            {
                PreventiveSchedulerName = "Test PM Schedule",
                MachineGroupId = machineGroupId,
                MachineGroup = null!,
                DepartmentId = 1,
                MaintenanceCategoryId = miscIds[0],
                MiscMaintenanceCategory = null!,
                ScheduleId = miscIds[1],
                MiscSchedule = null!,
                FrequencyTypeId = miscIds[2],
                MiscFrequencyType = null!,
                FrequencyInterval = 30,
                FrequencyUnitId = miscIds[3],
                MiscFrequencyUnit = null!,
                EffectiveDate = new DateOnly(2025, 6, 1),
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

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[PreventiveSchedulerItems]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[PreventiveSchedulerActivity]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[PreventiveSchedulerDetail]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[PreventiveSchedulerHeader]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (mgId, miscIds) = await EnsureFKDependenciesAsync(ctx);

            var entity = BuildEntity(mgId, miscIds);
            var result = await CreateRepository(ctx).CreateAsync(entity);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (mgId, miscIds) = await EnsureFKDependenciesAsync(ctx);

            var entity = BuildEntity(mgId, miscIds);
            var created = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<MaintenanceManagement.Domain.Entities.PreventiveSchedulerHeader>().FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.PreventiveSchedulerName.Should().Be("Test PM Schedule");
            saved.FrequencyInterval.Should().Be(30);
            saved.GraceDays.Should().Be(3);
            saved.DownTimeEstimateHrs.Should().Be(4.5m);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (mgId, miscIds) = await EnsureFKDependenciesAsync(ctx);

            var entity = BuildEntity(mgId, miscIds);
            var created = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<MaintenanceManagement.Domain.Entities.PreventiveSchedulerHeader>().FirstOrDefaultAsync(x => x.Id == created.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (mgId, miscIds) = await EnsureFKDependenciesAsync(ctx);

            var entity = BuildEntity(mgId, miscIds);
            var created = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteEntity = BuildEntity(mgId, miscIds);
            var result = await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (mgId, miscIds) = await EnsureFKDependenciesAsync(ctx);

            var entity = BuildEntity(mgId, miscIds);
            var created = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteEntity = BuildEntity(mgId, miscIds);
            await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<MaintenanceManagement.Domain.Entities.PreventiveSchedulerHeader>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
