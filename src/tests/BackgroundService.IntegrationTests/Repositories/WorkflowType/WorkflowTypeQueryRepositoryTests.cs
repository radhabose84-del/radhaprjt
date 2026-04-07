using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.WorkflowType
{
    [Collection("DatabaseCollection")]
    public sealed class WorkflowTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkflowTypeQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkflowTypeQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WorkflowTypeQueryRepository(conn);
        }

        private async Task<int> SeedWorkflowTypeAsync(
            int moduleId = 1,
            int menuId = 100,
            byte hasLine = 1,
            byte isMultiselect = 0)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new WorkflowTypeCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Workflow.WorkflowType
            {
                ModuleId = moduleId,
                MenuId = menuId,
                HasLine = hasLine,
                IsMultiselect = isMultiselect,
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
        public async Task GetAllWorkflowTypeAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedWorkflowTypeAsync();

            var (items, total) = await CreateQueryRepo().GetAllWorkflowTypeAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllWorkflowTypeAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var wfId = await SeedWorkflowTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Workflow.WorkflowType { IsDeleted = IsDelete.Deleted };
            await new WorkflowTypeCommandRepository(ctx).DeleteAsync(wfId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllWorkflowTypeAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllWorkflowTypeAsync_Should_Return_Pagination()
        {
            await ClearTablesAsync();
            await SeedWorkflowTypeAsync(1, 101);
            await SeedWorkflowTypeAsync(1, 102);
            await SeedWorkflowTypeAsync(1, 103);

            var (items, total) = await CreateQueryRepo().GetAllWorkflowTypeAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID (via GetWorkflowByName) ---

        [Fact]
        public async Task GetWorkflowByName_Should_Return_Matching_Record()
        {
            await ClearTablesAsync();
            await SeedWorkflowTypeAsync(1, 555);

            var result = await CreateQueryRepo().GetWorkflowByName(555);

            result.Should().NotBeNull();
            result.MenuId.Should().Be(555);
        }

        [Fact]
        public async Task GetWorkflowByName_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWorkflowByName(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            await SeedWorkflowTypeAsync(1, 200);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(200, 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync(9999, 9999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var wfId = await SeedWorkflowTypeAsync(1, 300);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(300, 1, wfId);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var wfId = await SeedWorkflowTypeAsync();

            var result = await CreateQueryRepo().NotFoundAsync(wfId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetWorkflowTypeAutoComplete_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedWorkflowTypeAsync(1, 111);
            await SeedWorkflowTypeAsync(1, 222);

            var result = await CreateQueryRepo().GetWorkflowTypeAutoComplete("111");

            result.Should().HaveCount(1);
            result[0].MenuId.Should().Be(111);
        }

        [Fact]
        public async Task GetWorkflowTypeAutoComplete_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var wfId = await SeedWorkflowTypeAsync(1, 333);

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.WorkflowType.FirstAsync(x => x.Id == wfId);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetWorkflowTypeAutoComplete("333");

            result.Should().BeEmpty();
        }
    }
}
