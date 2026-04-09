using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.WorkflowType
{
    [Collection("DatabaseCollection")]
    public sealed class WorkflowTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkflowTypeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkflowTypeCommandRepository CreateRepository(NotificationDbContext ctx) => new(ctx);

        private static Domain.Entities.Workflow.WorkflowType BuildEntity(
            int moduleId = 1,
            int menuId = 100,
            byte hasLine = 1,
            byte isMultiselect = 0) =>
            new()
            {
                ModuleId = moduleId,
                MenuId = menuId,
                HasLine = hasLine,
                IsMultiselect = isMultiselect,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(NotificationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
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

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(2, 200, 1, 1));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WorkflowType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.ModuleId.Should().Be(2);
            saved.MenuId.Should().Be(200);
            saved.HasLine.Should().Be(1);
            saved.IsMultiselect.Should().Be(1);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WorkflowType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(3, 300);
            updated.Id = newId;
            var result = await CreateRepository(ctx).UpdateAsync(updated);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(5, 500, 0, 1);
            updated.Id = newId;
            await CreateRepository(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WorkflowType.FirstOrDefaultAsync(x => x.Id == newId);
            saved.ModuleId.Should().Be(5);
            saved.MenuId.Should().Be(500);
            saved.HasLine.Should().Be(0);
            saved.IsMultiselect.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity();
            entity.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().BeFalse();
        }

        // --- DELETE (Soft Delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.WorkflowType
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.WorkflowType
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.WorkflowType
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted.Should().NotBeNull();
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var deleteEntity = new Domain.Entities.Workflow.WorkflowType
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }
    }
}
