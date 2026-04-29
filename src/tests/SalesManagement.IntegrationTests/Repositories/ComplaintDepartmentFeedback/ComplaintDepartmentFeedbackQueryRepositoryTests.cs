using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintDepartmentFeedback;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintDepartmentFeedback
{
    /// <summary>
    /// Integration tests for ComplaintDepartmentFeedbackQueryRepository (Dapper).
    /// Tests simple validation methods that require same-module data only.
    /// GetAll/GetById are complex (cross-module lookups) so only basic flow is tested.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintDepartmentFeedbackQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintDepartmentFeedbackQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private ComplaintDepartmentFeedbackQueryRepository CreateRepo()
        {
            return new ComplaintDepartmentFeedbackQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                BuildDefaultUserLookup().Object,
                BuildDefaultPartyLookup().Object,
                BuildDefaultItemLookup().Object,
                BuildDefaultLotLookup().Object);
        }

        private static Mock<IUserLookup> BuildDefaultUserLookup()
        {
            var mock = new Mock<IUserLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<UserLookupDto>)ids.Select(id =>
                        new UserLookupDto { UserId = id, FirstName = "User", LastName = id.ToString() }).ToList());
            return mock;
        }

        private static Mock<IPartyLookup> BuildDefaultPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new PartyLookupDto { Id = id, PartyName = "Party " + id });
            return mock;
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemName = "Item " + id }).ToList());
            return mock;
        }

        private static Mock<ILotMasterLookup> BuildDefaultLotLookup()
        {
            var mock = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            mock.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());
            return mock;
        }

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CDFQ_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "DeptFeedback Query Test",
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
        /// Seeds: MiscType -> Misc statuses -> Complaint -> QCReview -> Assignment -> Feedback.
        /// Returns (feedbackId, assignmentId).
        /// </summary>
        private async Task<(int feedbackId, int assignmentId)> SeedFullChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "CDFQ_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_STATUS");
            var roleId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ROLE");
            var aStatusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ASTATUS");
            var fbStatusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_FBSTATUS");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CDFQ_C" + Guid.NewGuid().ToString("N")[..6],
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

            var feedback = new SalesManagement.Domain.Entities.ComplaintDepartmentFeedback
            {
                AssignmentId = assignment.Id,
                RootCauseText = "Test root cause",
                CorrectiveAction = "Test corrective",
                FeedbackStatusId = fbStatusId,
                ReworkCount = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintDepartmentFeedback.AddAsync(feedback);
            await ctx.SaveChangesAsync();

            return (feedback.Id, assignment.Id);
        }

        /// <summary>Seeds just the assignment chain (no feedback). Returns assignmentId.</summary>
        private async Task<int> SeedAssignmentOnlyAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "CDFQ_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_STATUS");
            var roleId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ROLE");
            var aStatusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ASTATUS");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CDFQ_A" + Guid.NewGuid().ToString("N")[..6],
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

            return assignment.Id;
        }

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
        // NotFoundAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            var result = await CreateRepo().NotFoundAsync(99999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var (feedbackId, _) = await SeedFullChainAsync();

            var result = await CreateRepo().NotFoundAsync(feedbackId);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // AssignmentExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AssignmentExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            var (_, assignmentId) = await SeedFullChainAsync();

            var result = await CreateRepo().AssignmentExistsAsync(assignmentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AssignmentExistsAsync_Should_Return_False_When_NotExists()
        {
            var result = await CreateRepo().AssignmentExistsAsync(99999);
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // FeedbackAlreadyExistsForAssignmentAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task FeedbackAlreadyExistsForAssignmentAsync_Should_Return_True()
        {
            await ClearAsync();
            var (_, assignmentId) = await SeedFullChainAsync();

            var result = await CreateRepo().FeedbackAlreadyExistsForAssignmentAsync(assignmentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FeedbackAlreadyExistsForAssignmentAsync_Should_Return_False_When_No_Feedback()
        {
            await ClearAsync();
            var assignmentId = await SeedAssignmentOnlyAsync();

            var result = await CreateRepo().FeedbackAlreadyExistsForAssignmentAsync(assignmentId);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // MiscMasterExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Active()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var miscId = await EnsureMiscAsync(ctx, mtId, "CDFQ_MISCEX");

            var result = await CreateRepo().MiscMasterExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(99999);
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // GetByIdAsync (simplified)
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(99999);
            result.Should().BeNull();
        }

        // ---------------------------------------------------------------------------
        // GetByAssignmentIdAsync (simplified)
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByAssignmentIdAsync_Should_Return_Null_When_No_Assignment()
        {
            var result = await CreateRepo().GetByAssignmentIdAsync(99999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAssignmentIdAsync_Should_Return_Dto_When_Assignment_Has_No_Feedback()
        {
            await ClearAsync();
            var assignmentId = await SeedAssignmentOnlyAsync();

            // GetByAssignmentIdAsync returns a DTO even when no feedback exists
            // (it joins from assignment and left-joins feedback)
            var result = await CreateRepo().GetByAssignmentIdAsync(assignmentId);

            result.Should().NotBeNull();
            result!.AssignmentId.Should().Be(assignmentId);
            result.FeedbackStatusName.Should().Be("Pending");
        }

        // ---------------------------------------------------------------------------
        // GetAllAsync — "QC Accepted" parent-status filter
        // (Only feedbacks whose parent ComplaintQCReview has Code = 'QC Accepted'
        //  should surface to responsible-person users.)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Seeds full chain and allows caller to control the QC Review's ComplaintStatusId.
        /// Pass "QC Accepted" / "QC Rejected" / null.
        /// </summary>
        private async Task<(int feedbackId, int assignmentId)> SeedFullChainWithQcStatusAsync(
            string? qcReviewStatusCode,
            int responsiblePersonId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "CDFQ_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_STATUS");
            var roleId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ROLE");
            var aStatusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_ASTATUS");
            var fbStatusId = await EnsureMiscAsync(ctx, mtId, "CDFQ_FBSTATUS");

            int? qcrStatusId = null;
            if (!string.IsNullOrWhiteSpace(qcReviewStatusCode))
                qcrStatusId = await EnsureMiscAsync(ctx, mtId, qcReviewStatusCode);

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CDFQ_S" + Guid.NewGuid().ToString("N")[..6],
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
                ComplaintStatusId = qcrStatusId,
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
                ResponsiblePersonId = responsiblePersonId,
                IsMandatory = true,
                AssignmentStatusId = aStatusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReviewAssignment.AddAsync(assignment);
            await ctx.SaveChangesAsync();

            var feedback = new SalesManagement.Domain.Entities.ComplaintDepartmentFeedback
            {
                AssignmentId = assignment.Id,
                RootCauseText = "Test root cause",
                CorrectiveAction = "Test corrective",
                FeedbackStatusId = fbStatusId,
                ReworkCount = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintDepartmentFeedback.AddAsync(feedback);
            await ctx.SaveChangesAsync();

            return (feedback.Id, assignment.Id);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Feedback_When_QCReview_Status_Is_QC_Accepted()
        {
            await ClearAsync();
            var (_, assignmentId) = await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 1);

            var (data, totalCount) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 1);

            totalCount.Should().Be(1);
            data.Should().ContainSingle(d => d.AssignmentId == assignmentId);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_Feedback_When_QCReview_Status_Is_QC_Rejected()
        {
            await ClearAsync();
            await SeedFullChainWithQcStatusAsync("QC Rejected", responsiblePersonId: 1);

            var (data, totalCount) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 1);

            totalCount.Should().Be(0);
            data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_Feedback_When_QCReview_Status_Is_Null()
        {
            await ClearAsync();
            // Null ComplaintStatusId => INNER JOIN on MiscMaster fails => row excluded.
            await SeedFullChainWithQcStatusAsync(qcReviewStatusCode: null, responsiblePersonId: 1);

            var (data, totalCount) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 1);

            totalCount.Should().Be(0);
            data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Respect_ResponsiblePerson_Filter_With_QC_Accepted_Status()
        {
            await ClearAsync();
            var (_, assignmentForUser1) = await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 1);
            await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 2);

            var (data, totalCount) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 1);

            totalCount.Should().Be(1);
            data.Should().ContainSingle(d => d.AssignmentId == assignmentForUser1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_Feedback_When_Status_Is_Mixed_Only_Approved_Remain()
        {
            await ClearAsync();
            var (_, approvedAssignment) = await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 1);
            await SeedFullChainWithQcStatusAsync("QC Rejected", responsiblePersonId: 1);
            await SeedFullChainWithQcStatusAsync(qcReviewStatusCode: null, responsiblePersonId: 1);

            var (data, totalCount) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 1);

            totalCount.Should().Be(1);
            data.Should().ContainSingle(d => d.AssignmentId == approvedAssignment);
        }

        // ---------------------------------------------------------------------------
        // Fix A — verify that the GetAllAsync SQL projection includes the
        // ComplaintHeader.Id (so the frontend can chain /all-for-reviewer →
        // /by-complaint-full/{complaintHeaderId} without an extra lookup).
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_Should_Project_ComplaintHeaderId_From_Joined_Header()
        {
            await ClearAsync();
            var (_, assignmentId) = await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 7);

            // Find the expected ComplaintHeader.Id via the assignment chain
            int expectedComplaintHeaderId;
            await using (var verifyCtx = _fixture.CreateFreshDbContext())
            {
                expectedComplaintHeaderId = await verifyCtx.ComplaintQCReviewAssignment
                    .Where(a => a.Id == assignmentId)
                    .Join(verifyCtx.ComplaintQCReview, a => a.ComplaintQCReviewId, r => r.Id, (a, r) => r.ComplaintHeaderId)
                    .FirstAsync();
            }

            var (data, _) = await CreateRepo().GetAllAsync(1, 10, null, null, responsiblePersonId: 7);

            data.Should().ContainSingle(d => d.AssignmentId == assignmentId);
            var row = data.Single(d => d.AssignmentId == assignmentId);
            row.ComplaintHeaderId.Should().Be(expectedComplaintHeaderId,
                "Fix A: GetAllAsync must project ch.Id AS ComplaintHeaderId so the frontend can drill in");
            row.ComplaintHeaderId.Should().BeGreaterThan(0,
                "the seeded ComplaintHeader has a real identity-generated id");
        }

        [Fact]
        public async Task GetByComplaintIdAsync_Should_Project_ComplaintHeaderId_From_Joined_Header()
        {
            await ClearAsync();
            var (_, assignmentId) = await SeedFullChainWithQcStatusAsync("QC Accepted", responsiblePersonId: 8);

            int expectedComplaintHeaderId;
            await using (var verifyCtx = _fixture.CreateFreshDbContext())
            {
                expectedComplaintHeaderId = await verifyCtx.ComplaintQCReviewAssignment
                    .Where(a => a.Id == assignmentId)
                    .Join(verifyCtx.ComplaintQCReview, a => a.ComplaintQCReviewId, r => r.Id, (a, r) => r.ComplaintHeaderId)
                    .FirstAsync();
            }

            var data = await CreateRepo().GetByComplaintIdAsync(expectedComplaintHeaderId);

            data.Should().NotBeEmpty();
            data.Should().AllSatisfy(d => d.ComplaintHeaderId.Should().Be(expectedComplaintHeaderId,
                "Fix A: GetByComplaintIdAsync must also surface ComplaintHeaderId on each row"));
        }
    }
}
