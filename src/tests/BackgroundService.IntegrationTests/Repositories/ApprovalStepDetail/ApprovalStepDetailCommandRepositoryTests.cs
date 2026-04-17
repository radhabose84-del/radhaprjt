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
    public sealed class ApprovalStepDetailCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalStepDetailCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApprovalStepDetailCommandRepository CreateRepository(NotificationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "ASDTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Approval Step Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "ASDSTEP")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Approval Step",
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

        private static Domain.Entities.Workflow.ApprovalStepDetail BuildEntity(
            int workflowTypeId,
            int approvalStepId,
            int targetTypeId,
            int stepOrder = 1) =>
            new()
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
                ApprovalStepUnitMappings = new List<ApprovalStepUnitMapping>(),
                ApprovalStepDepartmentMappings = new List<ApprovalStepDepartmentMapping>()
            };

        private async Task ClearTablesAsync(NotificationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP01");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET01");
            var wfTypeId = await SeedWorkflowTypeAsync();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP02");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET02");
            var wfTypeId = await SeedWorkflowTypeAsync(200);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId, 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalStepDetail.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.WorkFlowTypeId.Should().Be(wfTypeId);
            saved.ApprovalStepId.Should().Be(approvalStepId);
            saved.TargetTypeId.Should().Be(targetTypeId);
            saved.StepOrder.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP03");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET03");
            var wfTypeId = await SeedWorkflowTypeAsync(300);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalStepDetail.FirstOrDefaultAsync(x => x.Id == newId);

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
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP04");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET04");
            var wfTypeId = await SeedWorkflowTypeAsync(400);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(wfTypeId, approvalStepId, targetTypeId, 10);
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
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP05");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET05");
            var wfTypeId = await SeedWorkflowTypeAsync(500);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(wfTypeId, approvalStepId, targetTypeId, 20);
            updated.Id = newId;
            updated.StopOnFirstMatch = 1;
            updated.IsEdit = 1;
            await CreateRepository(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ApprovalStepDetail.FirstOrDefaultAsync(x => x.Id == newId);
            saved.StepOrder.Should().Be(20);
            saved.StopOnFirstMatch.Should().Be(1);
            saved.IsEdit.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP06");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET06");
            var wfTypeId = await SeedWorkflowTypeAsync(600);

            var entity = BuildEntity(wfTypeId, approvalStepId, targetTypeId);
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
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP07");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET07");
            var wfTypeId = await SeedWorkflowTypeAsync(700);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.ApprovalStepDetail
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
            var approvalStepId = await SeedMiscMasterAsync(miscTypeId, "STEP08");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TARGET08");
            var wfTypeId = await SeedWorkflowTypeAsync(800);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(wfTypeId, approvalStepId, targetTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Workflow.ApprovalStepDetail
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ApprovalStepDetail
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

            var deleteEntity = new Domain.Entities.Workflow.ApprovalStepDetail
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }
    }
}
