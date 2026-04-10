using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalStepDetails;
using BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.ApprovalStepDetail
{
    [Collection("DatabaseCollection")]
    public sealed class ApprovalStepDetailQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalStepDetailQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApprovalStepDetailQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ApprovalStepDetailQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "ASDQTYP")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Approval Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "ASDQSTP")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Step",
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

        private async Task<int> SeedApprovalStepDetailAsync(
            int workflowTypeId,
            int approvalStepId,
            int targetTypeId,
            int stepOrder = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ApprovalStepDetailCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Workflow.ApprovalStepDetail
            {
                WorkFlowTypeId = workflowTypeId,
                StepOrder = stepOrder,
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
        public async Task GetAllApprovalStepDetailAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "ALLSTP1");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "ALLTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(100);
            await SeedApprovalStepDetailAsync(wfTypeId, approvalStepId, targetTypeId);

            var (items, total) = await CreateQueryRepo().GetAllApprovalStepDetailAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllApprovalStepDetailAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "DELSTP1");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "DELTGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(101);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, approvalStepId, targetTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Workflow.ApprovalStepDetail { IsDeleted = IsDelete.Deleted };
            await new ApprovalStepDetailCommandRepository(ctx).DeleteAsync(asdId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllApprovalStepDetailAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllApprovalStepDetailAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var step1Id = await SeedMiscMasterAsync(miscTypeId, "SRCHST1");
            var step2Id = await SeedMiscMasterAsync(miscTypeId, "OTHRSTP");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "SRCHTGT");
            var wfTypeId = await SeedWorkflowTypeAsync(102);
            await SeedApprovalStepDetailAsync(wfTypeId, step1Id, targetTypeId, 1);
            await SeedApprovalStepDetailAsync(wfTypeId, step2Id, targetTypeId, 2);

            var (items, total) = await CreateQueryRepo().GetAllApprovalStepDetailAsync(1, 10, "SRCHST1");

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_With_Mappings()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "GBISTP1");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "GBITGT1");
            var wfTypeId = await SeedWorkflowTypeAsync(103);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, approvalStepId, targetTypeId);

            var result = await CreateQueryRepo().GetByIdAsync(asdId);

            result.Should().NotBeNull();
            result.WorkFlowTypeId.Should().Be(wfTypeId);
            result.ApprovalStepId.Should().Be(approvalStepId);
            result.TargetTypeId.Should().Be(targetTypeId);
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
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "NFSTP01");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "NFTGT01");
            var wfTypeId = await SeedWorkflowTypeAsync(104);
            var asdId = await SeedApprovalStepDetailAsync(wfTypeId, approvalStepId, targetTypeId);

            var result = await CreateQueryRepo().NotFoundAsync(asdId);

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
