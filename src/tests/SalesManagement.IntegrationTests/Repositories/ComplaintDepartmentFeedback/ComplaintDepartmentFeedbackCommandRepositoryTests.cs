using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintDepartmentFeedback;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintDepartmentFeedback
{
    /// <summary>
    /// Integration tests for ComplaintDepartmentFeedbackCommandRepository.
    /// Requires: ComplaintHeader -> ComplaintQCReview -> ComplaintQCReviewAssignment (seeded).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintDepartmentFeedbackCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintDepartmentFeedbackCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ComplaintDepartmentFeedbackCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ComplaintDepartmentFeedbackCommandRepository(ctx);

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CDFC_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "DeptFeedback Cmd Test",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        /// <summary>
        /// Seeds full chain: MiscType -> MiscMaster -> ComplaintHeader -> QCReview -> Assignment.
        /// Returns (assignmentId, feedbackStatusId).
        /// </summary>
        private async Task<(int assignmentId, int feedbackStatusId)> SeedFullChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "CDFC_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "CDFC_STATUS");
            var roleId = await EnsureMiscAsync(ctx, mtId, "CDFC_ROLE");
            var aStatusId = await EnsureMiscAsync(ctx, mtId, "CDFC_ASTATUS");
            var fbStatusId = await EnsureMiscAsync(ctx, mtId, "CDFC_FBSTATUS");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CDFC_C" + Guid.NewGuid().ToString("N")[..6],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();

            var review = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                ComplaintHeaderId = complaint.Id,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReview.AddAsync(review);
            await ctx.SaveChangesAsync();

            var assignment = new SalesManagement.Domain.Entities.ComplaintQCReviewAssignment
            {
                ComplaintQCReviewId = review.Id,
                RoleId = roleId,
                ResponsiblePersonId = 1,
                IsMandatory = true,
                AssignmentStatusId = aStatusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReviewAssignment.AddAsync(assignment);
            await ctx.SaveChangesAsync();

            return (assignment.Id, fbStatusId);
        }

        private SalesManagement.Domain.Entities.ComplaintDepartmentFeedback BuildEntity(
            int assignmentId, int feedbackStatusId)
            => new SalesManagement.Domain.Entities.ComplaintDepartmentFeedback
            {
                AssignmentId = assignmentId,
                RootCauseText = "Root cause text",
                CorrectiveAction = "Corrective action",
                PreventiveAction = "Preventive action",
                Remarks = "Test feedback",
                FeedbackStatusId = feedbackStatusId,
                ReworkCount = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearTablesAsync(
            "Sales.ComplaintDepartmentFeedback",
            "Sales.ComplaintFeedbackAttachment",
            "Sales.ComplaintQCReviewAssignment",
            "Sales.ComplaintResolution",
            "Sales.ComplaintQCReview",
            "Sales.ComplaintDetailNature",
            "Sales.ComplaintDetail",
            "Sales.ComplaintAttachment",
            "Sales.ComplaintHeader");

        // ---------------------------------------------------------------------------
        // CreateAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            var (assignmentId, fbStatusId) = await SeedFullChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assignmentId, fbStatusId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            var (assignmentId, fbStatusId) = await SeedFullChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(assignmentId, fbStatusId);
            entity.RootCauseText = "Specific root cause";
            entity.CorrectiveAction = "Fix it";

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintDepartmentFeedback.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.AssignmentId.Should().Be(assignmentId);
            saved.RootCauseText.Should().Be("Specific root cause");
            saved.CorrectiveAction.Should().Be("Fix it");
            saved.FeedbackStatusId.Should().Be(fbStatusId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            var (assignmentId, fbStatusId) = await SeedFullChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assignmentId, fbStatusId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintDepartmentFeedback.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // UpdateStatusAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task UpdateStatusAsync_Should_Update_FeedbackStatus()
        {
            await ClearAsync();
            var (assignmentId, fbStatusId) = await SeedFullChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(assignmentId, fbStatusId));
            ctx.ChangeTracker.Clear();

            // Create a new status to update to
            var mtId = await EnsureMiscTypeAsync(ctx, "CDFC_MT");
            var newStatusId = await EnsureMiscAsync(ctx, mtId, "CDFC_REWORK");

            var resultId = await CreateRepository(ctx).UpdateStatusAsync(id, newStatusId, "Rework needed", 1);

            resultId.Should().Be(id);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.ComplaintDepartmentFeedback.FirstOrDefaultAsync(x => x.Id == id);
            saved!.FeedbackStatusId.Should().Be(newStatusId);
            saved.ReworkReason.Should().Be("Rework needed");
            saved.ReworkCount.Should().Be(1);
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateStatusAsync(99999, 1, "nope", 0);

            result.Should().Be(0);
        }

        // ---------------------------------------------------------------------------
        // UpdateAssignmentStatusAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAssignmentStatusAsync_Should_Update_Status()
        {
            await ClearAsync();
            var (assignmentId, _) = await SeedFullChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx, "CDFC_MT");
            var newAStatusId = await EnsureMiscAsync(ctx, mtId, "CDFC_SUBMITTED");

            var resultId = await CreateRepository(ctx).UpdateAssignmentStatusAsync(assignmentId, newAStatusId);

            resultId.Should().Be(assignmentId);
        }

        [Fact]
        public async Task UpdateAssignmentStatusAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAssignmentStatusAsync(99999, 1);

            result.Should().Be(0);
        }

        // ---------------------------------------------------------------------------
        // DeleteAttachmentAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task DeleteAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteAttachmentAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
