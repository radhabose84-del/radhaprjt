using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRules;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalStepDetails;
using BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.ApprovalRule
{
    [Collection("DatabaseCollection")]
    public sealed class ApprovalRuleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalRuleCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApprovalRuleCommandRepository CreateRepository(NotificationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "ARTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Approval Rule Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "ARMISC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Action",
                    SortOrder = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedWorkflowTypeAsync(int menuId = 100)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new WorkflowTypeCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Workflow.WorkflowType
                {
                    ModuleId = 1,
                    MenuId = menuId,
                    HasLine = 1,
                    IsMultiselect = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedApprovalStepDetailAsync(int wfTypeId, int approvalStepId, int targetTypeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new ApprovalStepDetailCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Workflow.ApprovalStepDetail
                {
                    WorkFlowTypeId = wfTypeId,
                    StepOrder = 1,
                    ApprovalStepId = approvalStepId,
                    TargetTypeId = targetTypeId,
                    TargetValueId = 1,
                    StopOnFirstMatch = 0,
                    IsEdit = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted,
                    ApprovalStepUnitMappings = new List<ApprovalStepUnitMapping>
                    {
                        new ApprovalStepUnitMapping { UnitId = 1 }
                    },
                    ApprovalStepDepartmentMappings = new List<ApprovalStepDepartmentMapping>()
                });
        }

        private static Domain.Entities.Workflow.ApprovalRule BuildEntity(
            int approvalStepDetailId,
            int actionId,
            int priority = 1) =>
            new()
            {
                ApprovalStepDetailId = approvalStepDetailId,
                ActionId = actionId,
                Priority = priority,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Conditions = new List<ApprovalRuleCondition>()
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
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION1");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP01");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG01");
            var wfTypeId = await SeedWorkflowTypeAsync(100);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION2");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP02");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG02");
            var wfTypeId = await SeedWorkflowTypeAsync(200);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId, 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalRule.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.ApprovalStepDetailId.Should().Be(asdId);
            saved.ActionId.Should().Be(actionId);
            saved.Priority.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION3");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP03");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG03");
            var wfTypeId = await SeedWorkflowTypeAsync(300);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalRule.FirstOrDefaultAsync(x => x.Id == newId);

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
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION4");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP04");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG04");
            var wfTypeId = await SeedWorkflowTypeAsync(400);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(asdId, actionId, 10);
            updated.Id = newId;
            var result = await CreateRepository(ctx).UpdateAsync(updated);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION5");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP05");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG05");
            var wfTypeId = await SeedWorkflowTypeAsync(500);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(asdId, actionId, 99);
            updated.Id = newId;
            updated.EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(5));
            await CreateRepository(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalRule.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Priority.Should().Be(99);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION6");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP06");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG06");
            var wfTypeId = await SeedWorkflowTypeAsync(600);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);

            var entity = BuildEntity(asdId, actionId);
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
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION7");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP07");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG07");
            var wfTypeId = await SeedWorkflowTypeAsync(700);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.ApprovalRule
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
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "ACTION8");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "STEP08");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "TARG08");
            var wfTypeId = await SeedWorkflowTypeAsync(800);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(asdId, actionId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.ApprovalRule
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ApprovalRule
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

            var deleteEntity = new Domain.Entities.Workflow.ApprovalRule
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }
    }
}
