using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Complaint
{
    /// <summary>
    /// Integration tests for ComplaintCommandRepository.
    /// ComplaintHeader has a complex CreateAsync(entity, typeId) that increments Finance.DocumentSequence.
    /// Tests validate Create, SoftDelete, and attachment operations.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ComplaintCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ComplaintCommandRepository(ctx);

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CCR_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Complaint Cmd Test Type",
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

        private SalesManagement.Domain.Entities.ComplaintHeader BuildEntity(
            int? statusId = null,
            string complaintNumber = "CCR_C001",
            int customerId = 100)
            => new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = complaintNumber,
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = customerId,
                StatusId = statusId,
                Remarks = "Test complaint",
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
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_STATUS");

            var entity = BuildEntity(statusId, "CCR_C001");
            // typeId = 1 → first Finance.DocumentSequence row (seeded by DbFixture)
            var newId = await CreateRepository(ctx).CreateAsync(entity, 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_PERSIST");

            var entity = BuildEntity(statusId, "CCR_PERST", customerId: 42);
            entity.CreditLimit = 50000;
            entity.Remarks = "Persist test";

            var newId = await CreateRepository(ctx).CreateAsync(entity, 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ComplaintNumber.Should().Be("CCR_PERST");
            saved.CustomerId.Should().Be(42);
            saved.CreditLimit.Should().Be(50000);
            saved.Remarks.Should().Be("Persist test");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_AUDIT");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_AUD01"), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // SoftDeleteAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_EntityExists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD1");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD01"), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD2");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD02"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_AlreadyDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD3");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD03"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // AddAttachmentAsync / DeleteAttachmentAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AddAttachmentAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_ATT1");
            var complaintId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_ATT01"), 1);
            ctx.ChangeTracker.Clear();

            var attachment = new SalesManagement.Domain.Entities.ComplaintAttachment
            {
                ComplaintHeaderId = complaintId,
                FileName = "test.pdf",
                FilePath = "/uploads/test.pdf",
                FileType = "application/pdf",
                FileSize = 1024,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var attId = await CreateRepository(ctx).AddAttachmentAsync(attachment);

            attId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_ATT2");
            var complaintId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_ATT02"), 1);
            ctx.ChangeTracker.Clear();

            var attachment = new SalesManagement.Domain.Entities.ComplaintAttachment
            {
                ComplaintHeaderId = complaintId,
                FileName = "del.pdf",
                FilePath = "/uploads/del.pdf",
                FileType = "application/pdf",
                FileSize = 512,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var attId = await CreateRepository(ctx).AddAttachmentAsync(attachment);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAttachmentAsync(attId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteAttachmentAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // EnsureResolutionDraftIfQCAcceptedAsync (Option B — no mandatory-feedback gate)
        //
        // Validates the business decision of 2026-04-24: as soon as the parent
        // ComplaintHeader is at 'QC Accepted' the draft resolution must be seeded,
        // irrespective of whether mandatory QC assignments have been Submitted.
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Seeds the MiscMaster rows that EnsureResolutionDraftIfQCAcceptedAsync needs:
        /// (QCComplaintStatus / 'QC Accepted'), (ResolutionType / 'No Action'),
        /// (ClosureStatus / 'Open'). Returns the MiscMaster Id of 'QC Accepted'.
        /// </summary>
        private async Task<int> SeedResolutionSeedMasterDataAsync(ApplicationDbContext ctx)
        {
            var qcMtId = await EnsureMiscTypeAsync(ctx, "QCComplaintStatus");
            var rtMtId = await EnsureMiscTypeAsync(ctx, "ResolutionType");
            var csMtId = await EnsureMiscTypeAsync(ctx, "ClosureStatus");

            var qcAcceptedId = await EnsureMiscAsync(ctx, qcMtId, "QC Accepted");
            await EnsureMiscAsync(ctx, rtMtId, "No Action");
            await EnsureMiscAsync(ctx, csMtId, "Open");

            return qcAcceptedId;
        }

        [Fact]
        public async Task EnsureResolutionDraftIfQCAcceptedAsync_HeaderQcAcceptedWithPendingMandatoryAssignment_CreatesDraft()
        {
            await ClearAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var qcAcceptedId = await SeedResolutionSeedMasterDataAsync(ctx);

            // Aux status for the pending assignment — deliberately NOT 'Submitted'.
            var auxMtId = await EnsureMiscTypeAsync(ctx, "CCR_AUX");
            var pendingStatusId = await EnsureMiscAsync(ctx, auxMtId, "Pending");
            var roleId = await EnsureMiscAsync(ctx, auxMtId, "CCR_ROLE");
            var pvId = await EnsureMiscAsync(ctx, auxMtId, "CCR_PV");

            var header = BuildEntity(statusId: qcAcceptedId, complaintNumber: "CCR_OPTB_1");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            var review = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                ComplaintHeaderId = header.Id,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReview.AddAsync(review);
            await ctx.SaveChangesAsync();

            // Mandatory assignment, still Pending — pre-Option-B this would have blocked Gate 2.
            var assignment = new SalesManagement.Domain.Entities.ComplaintQCReviewAssignment
            {
                ComplaintQCReviewId = review.Id,
                RoleId = roleId,
                ResponsiblePersonId = 1,
                IsMandatory = true,
                AssignmentStatusId = pendingStatusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReviewAssignment.AddAsync(assignment);
            await ctx.SaveChangesAsync();

            // Act
            await CreateRepository(ctx).EnsureResolutionDraftIfQCAcceptedAsync(header.Id, 7, "tester", "127.0.0.1", CancellationToken.None);

            // Assert
            ctx.ChangeTracker.Clear();
            var seeded = await ctx.ComplaintResolution
                .Where(cr => cr.ComplaintHeaderId == header.Id && cr.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();

            seeded.Should().HaveCount(1,
                "Option B: draft must be created on 'QC Accepted' even if mandatory feedback is still Pending");

            // Audit override must persist — raw SQL bypasses ApplicationDbContext.UpdateIpFields()
            seeded[0].CreatedBy.Should().Be(7);
            seeded[0].CreatedByName.Should().Be("tester");
            seeded[0].CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task EnsureResolutionDraftIfQCAcceptedAsync_HeaderNotQcAccepted_DoesNotCreateDraft()
        {
            await ClearAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedResolutionSeedMasterDataAsync(ctx);

            // Header is at 'QC Rejected' — Gate 1 must block.
            var qcMtId = await EnsureMiscTypeAsync(ctx, "QCComplaintStatus");
            var qcRejectedId = await EnsureMiscAsync(ctx, qcMtId, "QC Rejected");

            var header = BuildEntity(statusId: qcRejectedId, complaintNumber: "CCR_OPTB_2");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            // Act
            await CreateRepository(ctx).EnsureResolutionDraftIfQCAcceptedAsync(header.Id, 7, "tester", "127.0.0.1", CancellationToken.None);

            // Assert
            ctx.ChangeTracker.Clear();
            var resolutionCount = await ctx.ComplaintResolution
                .CountAsync(cr => cr.ComplaintHeaderId == header.Id && cr.IsDeleted == IsDelete.NotDeleted);

            resolutionCount.Should().Be(0, "Gate 1 still blocks when the header is not 'QC Accepted'");
        }

        [Fact]
        public async Task EnsureResolutionDraftIfQCAcceptedAsync_CalledTwiceOnQcAcceptedHeader_DoesNotCreateDuplicate()
        {
            await ClearAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var qcAcceptedId = await SeedResolutionSeedMasterDataAsync(ctx);

            var header = BuildEntity(statusId: qcAcceptedId, complaintNumber: "CCR_OPTB_3");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            // Act — invoke twice
            await CreateRepository(ctx).EnsureResolutionDraftIfQCAcceptedAsync(header.Id, 7, "tester", "127.0.0.1", CancellationToken.None);
            await CreateRepository(ctx).EnsureResolutionDraftIfQCAcceptedAsync(header.Id, 7, "tester", "127.0.0.1", CancellationToken.None);

            // Assert — Gate 3 (no-duplicate) holds
            ctx.ChangeTracker.Clear();
            var resolutionCount = await ctx.ComplaintResolution
                .CountAsync(cr => cr.ComplaintHeaderId == header.Id && cr.IsDeleted == IsDelete.NotDeleted);

            resolutionCount.Should().Be(1, "Gate 3 must still prevent duplicate draft rows");
        }

        // ---------------------------------------------------------------------------
        // Audit override on workflow-approval consumer paths.
        // Raw SQL bypasses ApplicationDbContext.UpdateIpFields() so the approver's
        // identity (passed via msg.ModifiedBy/Name/IP) sticks instead of being
        // overwritten with consumer-context defaults (0/Anonymous).
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task UpdateApprovalStatusAsync_OverridesAuditFields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var mtId = await EnsureMiscTypeAsync(ctx, "ApprovalStatus");
            var pendingId = await EnsureMiscAsync(ctx, mtId, "Pending");
            var approvedId = await EnsureMiscAsync(ctx, mtId, "Approved");
            var pvMtId = await EnsureMiscTypeAsync(ctx, "PhysicalVerification");
            await EnsureMiscAsync(ctx, pvMtId, "Pending");

            var header = BuildEntity(statusId: pendingId, complaintNumber: "CCR_AUD_1");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            await CreateRepository(ctx).UpdateApprovalStatusAsync(
                header.Id, "Approved",
                modifiedBy: 379, modifiedByName: "Praveen", modifiedIP: "192.168.1.45",
                CancellationToken.None);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.ComplaintHeader.FirstAsync(x => x.Id == header.Id);
            saved.StatusId.Should().Be(approvedId);
            saved.ModifiedBy.Should().Be(379);
            saved.ModifiedByName.Should().Be("Praveen");
            saved.ModifiedIP.Should().Be("192.168.1.45");
        }

        [Fact]
        public async Task UpdateApprovalStatusAsync_AutoCreatesQCReviewWithAuditOverride()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var mtId = await EnsureMiscTypeAsync(ctx, "ApprovalStatus");
            var pendingId = await EnsureMiscAsync(ctx, mtId, "Pending");
            await EnsureMiscAsync(ctx, mtId, "Approved");
            var pvMtId = await EnsureMiscTypeAsync(ctx, "PhysicalVerification");
            await EnsureMiscAsync(ctx, pvMtId, "Pending");

            var header = BuildEntity(statusId: pendingId, complaintNumber: "CCR_AUD_2");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            await CreateRepository(ctx).UpdateApprovalStatusAsync(
                header.Id, "Approved",
                modifiedBy: 379, modifiedByName: "Praveen", modifiedIP: "192.168.1.45",
                CancellationToken.None);

            ctx.ChangeTracker.Clear();
            var qcReview = await ctx.ComplaintQCReview.FirstAsync(x => x.ComplaintHeaderId == header.Id);
            qcReview.CreatedBy.Should().Be(379);
            qcReview.CreatedByName.Should().Be("Praveen");
            qcReview.CreatedIP.Should().Be("192.168.1.45");
        }

        [Fact]
        public async Task UpdateQCReviewApprovalStatusAsync_OverridesAuditFields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var apprMtId = await EnsureMiscTypeAsync(ctx, "ApprovalStatus");
            var approvedId = await EnsureMiscAsync(ctx, apprMtId, "Approved");
            var qcMtId = await EnsureMiscTypeAsync(ctx, "QCComplaintStatus");
            var qcAcceptedId = await EnsureMiscAsync(ctx, qcMtId, "QC Accepted");
            var pvMtId = await EnsureMiscTypeAsync(ctx, "PhysicalVerification");
            var pvPendingId = await EnsureMiscAsync(ctx, pvMtId, "Pending");

            var header = BuildEntity(statusId: approvedId, complaintNumber: "CCR_AUD_3");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            // Seed QC Review with the QC's recorded decision = QC Accepted
            var review = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                ComplaintHeaderId = header.Id,
                PhysicalVerificationId = pvPendingId,
                ComplaintStatusId = qcAcceptedId,
                LabVerificationRequired = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReview.AddAsync(review);
            await ctx.SaveChangesAsync();

            await CreateRepository(ctx).UpdateQCReviewApprovalStatusAsync(
                header.Id, "Approved",
                modifiedBy: 376, modifiedByName: "Vinoth", modifiedIP: "192.168.1.50",
                CancellationToken.None);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.ComplaintHeader.FirstAsync(x => x.Id == header.Id);
            saved.StatusId.Should().Be(qcAcceptedId);
            saved.ModifiedBy.Should().Be(376);
            saved.ModifiedByName.Should().Be("Vinoth");
            saved.ModifiedIP.Should().Be("192.168.1.50");
        }

        [Fact]
        public async Task UpdateResolutionApprovalStatusAsync_OverridesAuditFields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var qcMtId = await EnsureMiscTypeAsync(ctx, "QCComplaintStatus");
            var qcAcceptedId = await EnsureMiscAsync(ctx, qcMtId, "QC Accepted");
            var csMtId = await EnsureMiscTypeAsync(ctx, "ClosureStatus");
            var openId = await EnsureMiscAsync(ctx, csMtId, "Open");

            var header = BuildEntity(statusId: qcAcceptedId, complaintNumber: "CCR_AUD_4");
            await ctx.ComplaintHeader.AddAsync(header);
            await ctx.SaveChangesAsync();

            await CreateRepository(ctx).UpdateResolutionApprovalStatusAsync(
                header.Id, "Approved",
                modifiedBy: 379, modifiedByName: "Praveen", modifiedIP: "192.168.1.45",
                CancellationToken.None);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.ComplaintHeader.FirstAsync(x => x.Id == header.Id);
            saved.StatusId.Should().Be(openId);
            saved.ModifiedBy.Should().Be(379);
            saved.ModifiedByName.Should().Be("Praveen");
            saved.ModifiedIP.Should().Be("192.168.1.45");
        }
    }
}
