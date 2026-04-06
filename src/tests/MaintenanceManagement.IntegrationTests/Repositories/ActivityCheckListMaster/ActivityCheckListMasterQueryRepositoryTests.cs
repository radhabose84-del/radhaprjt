using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster;
using MaintenanceManagement.Infrastructure.Repositories.ActivityMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ActivityCheckListMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityCheckListMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityCheckListMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityCheckListMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ActivityCheckListMasterQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
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

        private async Task<int> SeedActivityMasterAsync(int activityTypeId, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new ActivityMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ActivityMaster
                {
                    ActivityName = name,
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

        private async Task<int> SeedEntityAsync(int activityId, string checkList, int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new ActivityCheckListMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
                {
                    ActivityId = activityId,
                    ActivityCheckList = checkList,
                    UnitId = unitId,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ActivityCheckListMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ActivityMachineGroup]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ActivityMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllActivityCheckListMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM1");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity1");
            await SeedEntityAsync(activityId, "Check Bearings");

            var (items, total) = await CreateQueryRepo().GetAllActivityCheckListMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllActivityCheckListMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM2");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity2");
            var id = await SeedEntityAsync(activityId, "To Delete Check");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ActivityCheckListMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllActivityCheckListMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllActivityCheckListMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM3");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity3");
            await SeedEntityAsync(activityId, "Alpha Checklist");
            await SeedEntityAsync(activityId, "Beta Checklist");

            var (items, _) = await CreateQueryRepo().GetAllActivityCheckListMasterAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].ActivityChecklist.Should().Be("Alpha Checklist");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT4");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM4");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity4");
            var id = await SeedEntityAsync(activityId, "Inspect Valves");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ChecklistId.Should().Be(id);
            result.ActivityChecklist.Should().Be("Inspect Valves");
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
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT5");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM5");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity5");
            var id = await SeedEntityAsync(activityId, "Soft Deleted Check");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ActivityCheckListMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsCheckListAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT6");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM6");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity6");
            await SeedEntityAsync(activityId, "Existing Check");

            var exists = await CreateQueryRepo().AlreadyExistsCheckListAsync("Existing Check", activityId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsCheckListAsync_Should_Return_False_For_Different_Activity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT7");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM7");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity7");
            await SeedEntityAsync(activityId, "Unique Check");

            var exists = await CreateQueryRepo().AlreadyExistsCheckListAsync("Unique Check", 9999);

            exists.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_WorkOrder_Links()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ACLMQ_MT8");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "ACLMQ_MM8");
            var activityId = await SeedActivityMasterAsync(miscMasterId, "ACLMQ_Activity8");
            var id = await SeedEntityAsync(activityId, "No Link Check");

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
