using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster;
using MaintenanceManagement.Infrastructure.Repositories.ActivityMaster;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ActivityCheckListMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityCheckListMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityCheckListMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityCheckListMasterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "ACLM_MT1")
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

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "ACLM_MM1")
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

        private async Task<int> SeedActivityMasterAsync(int activityTypeId, string name = "ACLM_Activity", string suffix = "1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new ActivityMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ActivityMaster
                {
                    ActivityName = $"{name}_{suffix}",
                    Description = "Test Activity",
                    UnitId = 1,
                    DepartmentId = 1,
                    EstimatedDuration = 60,
                    ActivityType = activityTypeId,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private static MaintenanceManagement.Domain.Entities.ActivityCheckListMaster BuildEntity(
            int activityId,
            string checkList = "Check Oil Level",
            int unitId = 1) =>
            new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
            {
                ActivityId = activityId,
                ActivityCheckList = checkList,
                UnitId = unitId,
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
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_C1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_C1");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_C2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_C2");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId, "Inspect Gears", 1));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ActivityCheckListMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.ActivityId.Should().Be(activityId);
            saved.ActivityCheckList.Should().Be("Inspect Gears");
            saved.UnitId.Should().Be(1);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_C3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_C3");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ActivityCheckListMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

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
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_U1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_U1");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "U1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId, "Original Check"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
                {
                    Id = entity.Id,
                    ActivityId = activityId,
                    ActivityCheckList = "Updated Check",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active
                });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_U2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_U2");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "U2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId, "Before Update Check"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
                {
                    Id = entity.Id,
                    ActivityId = activityId,
                    ActivityCheckList = "After Update Check",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active
                });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ActivityCheckListMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);
            updated!.ActivityCheckList.Should().Be("After Update Check");
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_D1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_D1");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "D1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLM_MT_D2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLM_MM_D2");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLM_Act", "D2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(activityId, "To Delete Check"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ActivityCheckListMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
