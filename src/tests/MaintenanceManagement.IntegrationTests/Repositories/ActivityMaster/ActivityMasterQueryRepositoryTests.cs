using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ActivityMaster;
using MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ActivityMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ActivityMasterQueryRepository(conn, _fixture.IpMock.Object);
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

        private async Task<int> SeedEntityAsync(int activityTypeId, string name, int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new ActivityMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ActivityMaster
                {
                    ActivityName = name,
                    Description = "Test",
                    UnitId = unitId,
                    DepartmentId = 1,
                    EstimatedDuration = 60,
                    ActivityType = activityTypeId,
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
        public async Task GetAllActivityMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM1");
            await SeedEntityAsync(miscMasterId, "Query Activity 1");

            var (items, total) = await CreateQueryRepo().GetAllActivityMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllActivityMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM2");
            await SeedEntityAsync(miscMasterId, "Alpha Activity");
            await SeedEntityAsync(miscMasterId, "Beta Activity");

            var (items, _) = await CreateQueryRepo().GetAllActivityMasterAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].ActivityName.Should().Be("Alpha Activity");
        }

        [Fact]
        public async Task GetAllActivityMasterAsync_Should_Respect_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM3");
            await SeedEntityAsync(miscMasterId, "Activity One");
            await SeedEntityAsync(miscMasterId, "Activity Two");
            await SeedEntityAsync(miscMasterId, "Activity Three");

            var (items, total) = await CreateQueryRepo().GetAllActivityMasterAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT4");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM4");
            var id = await SeedEntityAsync(miscMasterId, "GetById Activity");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ActivityName.Should().Be("GetById Activity");
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetActivityMasterAutoComplete_Should_Return_Active_Records()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT5");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM5");
            await SeedEntityAsync(miscMasterId, "Autocomplete Activity");

            var results = await CreateQueryRepo().GetActivityMasterAutoComplete("Autocomplete");

            results.Should().HaveCount(1);
            results[0].ActivityName.Should().Be("Autocomplete Activity");
        }

        [Fact]
        public async Task GetActivityMasterAutoComplete_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT6");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM6");
            var id = await SeedEntityAsync(miscMasterId, "Inactive Activity");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.ActivityMaster.FindAsync(id);
            entity!.IsActive = BaseEntity.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().GetActivityMasterAutoComplete("Inactive");

            results.Should().BeEmpty();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT7");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM7");
            var id = await SeedEntityAsync(miscMasterId, "Existing Activity");

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }

        // --- FK COLUMN VALIDATION ---

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Activity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT8");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM8");
            var id = await SeedEntityAsync(miscMasterId, "FK Valid Activity");

            var exists = await CreateQueryRepo().FKColumnExistValidation(id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().FKColumnExistValidation(9999);

            exists.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_CheckList_Links()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT9");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM9");
            var id = await SeedEntityAsync(miscMasterId, "No Link Activity");

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_CheckList_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AMQ_MT10");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "AMQ_MM10");
            var activityId = await SeedEntityAsync(miscMasterId, "Linked Activity");

            // Add a checklist item linked to this activity
            await using var ctx = _fixture.CreateFreshDbContext();
            await new ActivityCheckListMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
                {
                    ActivityId = activityId,
                    ActivityCheckList = "Linked Check",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(activityId);

            result.Should().BeTrue();
        }
    }
}
