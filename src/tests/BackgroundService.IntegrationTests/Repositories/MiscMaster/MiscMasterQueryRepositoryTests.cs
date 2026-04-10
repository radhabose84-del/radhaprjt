using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "QMTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Test MiscType",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<Domain.Entities.Notification.MiscMaster> SeedMiscMasterAsync(
            int miscTypeId,
            string code = "MCODE01",
            string description = "Test MiscMaster")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Notification.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                DELETE FROM AppNotification.NotificationEventRule;
                DELETE FROM AppNotification.NotificationLevelHierarchy;
                DELETE FROM AppNotification.NotificationTemplate;
                DELETE FROM AppNotification.NotificationEventLog;
                DELETE FROM AppNotification.NotificationGroupMembers;
                DELETE FROM AppNotification.NotificationGroup;
                DELETE FROM AppNotification.NotificationConfig;
                DELETE FROM AppData.ApprovalRuleCondition;
                DELETE FROM AppData.RuleTargetOverride;
                DELETE FROM AppData.ApprovalRule;
                DELETE FROM AppData.ApprovalRequestLine;
                DELETE FROM AppData.ApprovalDocument;
                DELETE FROM AppData.ApprovalRequest;
                DELETE FROM AppData.ApprovalStepDepartmentMapping;
                DELETE FROM AppData.ApprovalStepUnitMapping;
                DELETE FROM AppData.ApprovalStepDetail;
                DELETE FROM AppData.WorkflowType;
                DELETE FROM AppData.MiscMaster;
                DELETE FROM AppData.MiscTypeMaster;
            ");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedMiscMasterAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.MiscMaster { IsDeleted = IsDelete.Deleted };
            await new MiscMasterCommandRepository(ctx).DeleteAsync(entity.Id, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedMiscMasterAsync(miscTypeId, "ALPHA", "Alpha Master");
            await SeedMiscMasterAsync(miscTypeId, "BETA", "Beta Master");

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("ALPHA");
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedMiscMasterAsync(miscTypeId, "MM01", "Master 1");
            await SeedMiscMasterAsync(miscTypeId, "MM02", "Master 2");
            await SeedMiscMasterAsync(miscTypeId, "MM03", "Master 3");

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId, "GETID01", "Get By Id");

            var result = await CreateQueryRepo().GetByIdAsync(entity.Id);

            result.Should().NotBeNull();
            result.Code.Should().Be("GETID01");
            result.Description.Should().Be("Get By Id");
            result.MiscTypeId.Should().Be(miscTypeId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.MiscMaster { IsDeleted = IsDelete.Deleted };
            await new MiscMasterCommandRepository(ctx).DeleteAsync(entity.Id, deleteEntity);

            var result = await CreateQueryRepo().GetByIdAsync(entity.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedMiscMasterAsync(miscTypeId, "DUP01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUP01", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOTEXIST", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId, "SELF01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("SELF01", miscTypeId, entity.Id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId);

            var result = await CreateQueryRepo().NotFoundAsync(entity.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }

        // --- FK COLUMN VALIDATION (MiscTypeExists) ---

        [Fact]
        public async Task FKColumnValidation_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var entity = await SeedMiscMasterAsync(miscTypeId);

            var result = await CreateQueryRepo().FKColumnValidation(entity.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnValidation_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().FKColumnValidation(9999);

            result.Should().BeFalse();
        }

        // --- AUTOCOMPLETE (GetMiscMaster) ---

        [Fact]
        public async Task GetMiscMaster_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("AUTOTP");
            await SeedMiscMasterAsync(miscTypeId, "AUTO01", "Auto Complete");
            await SeedMiscMasterAsync(miscTypeId, "NOMATCH", "No Match");

            var result = await CreateQueryRepo().GetMiscMaster("AUTO", "AUTOTP");

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("AUTO01");
        }

        [Fact]
        public async Task GetMiscMaster_Should_Return_All_For_Type_When_Empty_Pattern()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("ALLTP");
            await SeedMiscMasterAsync(miscTypeId, "ALL01", "All One");
            await SeedMiscMasterAsync(miscTypeId, "ALL02", "All Two");

            var result = await CreateQueryRepo().GetMiscMaster("", "ALLTP");

            result.Should().HaveCount(2);
        }

        // --- GET MAX SORT ORDER ---

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_Empty()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaxSortOrderAsync();

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_Value()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedMiscMasterAsync(miscTypeId, "MAX01");
            await SeedMiscMasterAsync(miscTypeId, "MAX02");

            var result = await CreateQueryRepo().GetMaxSortOrderAsync();

            result.Should().BeGreaterThan(0);
        }
    }
}
