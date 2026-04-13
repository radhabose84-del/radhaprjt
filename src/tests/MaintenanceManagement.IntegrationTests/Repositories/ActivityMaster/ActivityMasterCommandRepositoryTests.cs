using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ActivityMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ActivityMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityMasterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "AM_CMD_MT1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code, Description = "Activity Type",
                    IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "AM_CMD_MM1")
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

        private static MaintenanceManagement.Domain.Entities.ActivityMaster BuildEntity(
            int activityTypeId,
            string name = "Test Activity",
            int unitId = 1,
            int departmentId = 1) =>
            new MaintenanceManagement.Domain.Entities.ActivityMaster
            {
                ActivityName = name,
                Description = "Test Description",
                UnitId = unitId,
                DepartmentId = departmentId,
                EstimatedDuration = 60,
                ActivityType = activityTypeId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_C1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_C2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_C2");

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(miscMasterId, "Welding Activity", 1, 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ActivityMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.ActivityName.Should().Be("Welding Activity");
            saved.DepartmentId.Should().Be(2);
            saved.ActivityType.Should().Be(miscMasterId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_C3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "Audit Activity"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ActivityMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_RowsAffected_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_U1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_U1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "Original Activity"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new UpdateActivityMasterDto
            {
                ActivityId = entity.Id,
                ActivityName = "Updated Activity",
                Description = "Updated Description",
                DepartmentId = 1,
                UnitId = 1,
                EstimatedDuration = 90,
                ActivityType = miscMasterId,
                IsActive = BaseEntity.Status.Active,
                UpdateActivityMachineGroup = new List<UpdateActivityMachineGroupDto>()
            });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_U2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_U2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "Before Update Activity"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new UpdateActivityMasterDto
            {
                ActivityId = entity.Id,
                ActivityName = "After Update Activity",
                Description = "New Description",
                DepartmentId = 1,
                UnitId = 1,
                EstimatedDuration = 120,
                ActivityType = miscMasterId,
                IsActive = BaseEntity.Status.Active,
                UpdateActivityMachineGroup = new List<UpdateActivityMachineGroupDto>()
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ActivityMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);
            updated!.ActivityName.Should().Be("After Update Activity");
            updated.EstimatedDuration.Should().Be(120);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("AM_CMD_MT_U3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AM_CMD_MM_U3");

            var result = await CreateRepository(ctx).UpdateAsync(new UpdateActivityMasterDto
            {
                ActivityId = 9999,
                ActivityName = "No Such Activity",
                Description = "None",
                DepartmentId = 1,
                UnitId = 1,
                EstimatedDuration = 60,
                ActivityType = miscMasterId,
                IsActive = BaseEntity.Status.Active,
                UpdateActivityMachineGroup = new List<UpdateActivityMachineGroupDto>()
            });

            result.Should().Be(0);
        }
    }
}
