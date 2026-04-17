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
    public sealed class ApprovalRuleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalRuleQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApprovalRuleQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ApprovalRuleQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "ARQTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Rule Query Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "ARQMISC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Query Action",
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

        private async Task<int> SeedApprovalRuleAsync(int asdId, int actionId, int priority = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ApprovalRuleCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Workflow.ApprovalRule
            {
                ApprovalStepDetailId = asdId,
                ActionId = actionId,
                Priority = priority,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Conditions = new List<ApprovalRuleCondition>()
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllApprovalRuleAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "QACT01");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "QSTP01");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "QTGT01");
            var wfTypeId = await SeedWorkflowTypeAsync(100);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            await SeedApprovalRuleAsync(asdId, actionId);

            var (items, total) = await CreateQueryRepo().GetAllApprovalRuleAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllApprovalRuleAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "QDACT1");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "QDSTP1");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "QDTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(101);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var ruleId = await SeedApprovalRuleAsync(asdId, actionId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Workflow.ApprovalRule { IsDeleted = IsDelete.Deleted };
            await new ApprovalRuleCommandRepository(ctx).DeleteAsync(ruleId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllApprovalRuleAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllApprovalRuleAsync_Should_Return_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "QPACT1");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "QPSTP1");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "QPTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(102);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            await SeedApprovalRuleAsync(asdId, actionId, 1);
            await SeedApprovalRuleAsync(asdId, actionId, 2);
            await SeedApprovalRuleAsync(asdId, actionId, 3);

            var (items, total) = await CreateQueryRepo().GetAllApprovalRuleAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "QGACT1");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "QGSTP1");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "QGTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(103);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var ruleId = await SeedApprovalRuleAsync(asdId, actionId, 7);

            var result = await CreateQueryRepo().GetByIdAsync(ruleId);

            result.Should().NotBeNull();
            result.ActionId.Should().Be(actionId);
            result.ApprovalStepDetailId.Should().Be(asdId);
            result.Priority.Should().Be(7);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var actionId = await SeedMiscMasterAsync(miscTypeId, "QNACT1");
            var stepId = await SeedMiscMasterAsync(miscTypeId, "QNSTP1");
            var targetId = await SeedMiscMasterAsync(miscTypeId, "QNTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(104);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, stepId, targetId);
            var ruleId = await SeedApprovalRuleAsync(asdId, actionId);

            var result = await CreateQueryRepo().NotFoundAsync(ruleId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }
    }
}
