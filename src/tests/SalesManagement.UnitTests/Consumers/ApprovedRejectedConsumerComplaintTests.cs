using Contracts.Commands.Sales;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Lookups.Users;
using Contracts.Events.Notifications;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.Consumers;
using SalesManagement.Domain.Common;

namespace SalesManagement.UnitTests.Consumers
{
    // Covers the Complaint MO Approval branch of ApprovedRejectedConsumer.Consume —
    // specifically the UnitId resolution path that replaced the hardcoded UnitId = 37.
    // The InApp notification must route to the unit of the complaint's CREATOR, not the approver.
    public sealed class ApprovedRejectedConsumerComplaintTests
    {
        private readonly Mock<IInvoiceCommandRepository> _invoiceRepo = new(MockBehavior.Loose);
        private readonly Mock<ISalesOrderCommandRepository> _soRepo = new(MockBehavior.Loose);
        private readonly Mock<ISalesOrderAmendmentCommandRepository> _soAmendRepo = new(MockBehavior.Loose);
        private readonly Mock<ISalesQuotationCommandRepository> _sqRepo = new(MockBehavior.Loose);
        private readonly Mock<ISalesQuotationAmendmentCommandRepository> _sqAmendRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscRepo = new(MockBehavior.Loose);
        private readonly Mock<IStoHeaderCommandRepository> _stoRepo = new(MockBehavior.Loose);
        private readonly Mock<IDeliveryChallanCommandRepository> _dcRepo = new(MockBehavior.Loose);
        private readonly Mock<IComplaintCommandRepository> _complaintCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IComplaintQueryRepository> _complaintQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IComplaintQCReviewQueryRepository> _qcReviewQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IComplaintResolutionQueryRepository> _resolutionQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ApprovedRejectedConsumer>> _logger = new();
        private readonly Mock<IOfficerAgentUserLookup> _officerLookup = new(MockBehavior.Loose);
        private readonly Mock<IAppDataMiscMasterLookup> _appDataMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyDetailLookup> _partyLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _workflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _unitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentUserLookup> _departmentUserLookup = new(MockBehavior.Loose);

        public ApprovedRejectedConsumerComplaintTests()
        {
            // Default: no QC-reviewer resolves → OverrideTargetUserIds stays null
            // (dispatcher fallback). Individual tests override this.
            _departmentUserLookup
                .Setup(l => l.GetActiveUserIdsByApprovalStepTargetTypeAsync(
                    It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int>());
        }

        private ApprovedRejectedConsumer CreateSut() => new(
            _invoiceRepo.Object, _soRepo.Object, _soAmendRepo.Object, _sqRepo.Object,
            _sqAmendRepo.Object, _miscRepo.Object, _stoRepo.Object, _dcRepo.Object,
            _complaintCmdRepo.Object, _complaintQueryRepo.Object,
            _qcReviewQueryRepo.Object, _resolutionQueryRepo.Object,
            _mediator.Object,
            _logger.Object, _officerLookup.Object, _appDataMiscLookup.Object,
            _partyLookup.Object, _workflowLookup.Object, _unitLookup.Object,
            _departmentUserLookup.Object);

        private static (Mock<ConsumeContext<UpdateApprovedRejectedSalesCommand>> ctx, List<NotificationCreatedEvent> captured)
            BuildContext(int complaintId, string status, string? moduleTypeName = null)
        {
            var captured = new List<NotificationCreatedEvent>();
            var ctx = new Mock<ConsumeContext<UpdateApprovedRejectedSalesCommand>>(MockBehavior.Loose);

            ctx.SetupGet(c => c.Message).Returns(new UpdateApprovedRejectedSalesCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTransactionId = complaintId,
                ModuleTypeName = moduleTypeName ?? MiscEnumEntity.ComplaintModuleTypeName,
                Status = status,
                ModifiedBy = 999,
                ModifiedByName = "approver",
                ModifiedIP = "127.0.0.1"
            });
            ctx.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);

            ctx.Setup(c => c.Publish(It.IsAny<NotificationCreatedEvent>(), It.IsAny<CancellationToken>()))
               .Callback<NotificationCreatedEvent, CancellationToken>((e, _) => captured.Add(e))
               .Returns(Task.CompletedTask);

            return (ctx, captured);
        }

        private void StubNotificationEventTypeFound() =>
            _appDataMiscLookup
                .Setup(l => l.GetMiscMasterByNameAsync(
                    MiscEnumEntity.NotifEventTypeMiscType,
                    MiscEnumEntity.NotifEventTypeCreate))
                .ReturnsAsync(new MiscMasterLookupDto { Id = 100, Code = "Create", Description = "Create" });

        [Fact]
        public async Task Consume_ComplaintApproved_PublishesNotification_WithCreatorUnitId()
        {
            // Arrange — complaint 50 was created by user 7, who belongs to unit 12
            const int complaintId = 50;
            const int creatorUserId = 7;
            const int creatorUnitId = 12;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });

            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = creatorUnitId, UnitName = "Unit-A" } });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalApproved);

            // Act
            await CreateSut().Consume(ctx.Object);

            // Assert — exactly one InApp notification was published, routed to the creator's unit (NOT 37, NOT the approver)
            captured.Should().HaveCount(1);
            captured[0].UnitId.Should().Be(creatorUnitId);
            captured[0].ModuleName.Should().Be(MiscEnumEntity.NotifModuleComplaintMoApproval);
            captured[0].param1.Should().Be(complaintId.ToString());
        }

        [Fact]
        public async Task Consume_ComplaintApproved_ComplaintNotFound_PublishesWithUnitIdZero()
        {
            // Arrange — complaint lookup returns null (deleted, bad id, etc.)
            const int complaintId = 999;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync((ComplaintHeaderDto?)null);

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalApproved);

            // Act
            await CreateSut().Consume(ctx.Object);

            // Assert — UnitId falls back to 0 so the dispatcher drops the message instead of routing to a wrong unit
            captured.Should().HaveCount(1);
            captured[0].UnitId.Should().Be(0);
        }

        [Fact]
        public async Task Consume_ComplaintApproved_CreatorHasNoUnit_PublishesWithUnitIdZero()
        {
            // Arrange — complaint exists, but creator has no unit assignment
            const int complaintId = 60;
            const int creatorUserId = 8;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });

            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto>()); // empty

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalApproved);

            // Act
            await CreateSut().Consume(ctx.Object);

            // Assert — UnitId is 0 (warning logged, dispatcher will drop)
            captured.Should().HaveCount(1);
            captured[0].UnitId.Should().Be(0);
        }

        [Fact]
        public async Task Consume_ComplaintApproved_QcReviewersResolved_SetsOverrideTargetUserIds()
        {
            // Arrange — QC department team resolves to users 379 & 2443
            const int complaintId = 70;
            const int creatorUserId = 9;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });
            _departmentUserLookup
                .Setup(l => l.GetActiveUserIdsByApprovalStepTargetTypeAsync(
                    MiscEnumEntity.ComplaintQcReviewerTargetType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 379, 2443 });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalApproved);

            // Act
            await CreateSut().Consume(ctx.Object);

            // Assert — recipients overridden with the dynamically-resolved QC team
            captured.Should().HaveCount(1);
            captured[0].ModuleName.Should().Be(MiscEnumEntity.NotifModuleComplaintMoApproval);
            captured[0].OverrideTargetUserIds.Should().BeEquivalentTo(new[] { 379, 2443 });
        }

        [Fact]
        public async Task Consume_ComplaintApproved_NoQcReviewers_OverrideTargetUserIdsIsNull()
        {
            // Arrange — default stub returns empty QC list
            const int complaintId = 71;
            const int creatorUserId = 9;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalApproved);

            // Act
            await CreateSut().Consume(ctx.Object);

            // Assert — no override → dispatcher fallback (no regression)
            captured.Should().HaveCount(1);
            captured[0].OverrideTargetUserIds.Should().BeNull();
        }

        // ============================================================
        // Phase 3 — Rejection + QC Approved + Resolution Rejected branches
        // ============================================================

        [Fact]
        public async Task Consume_ComplaintRejected_PublishesToCreator()
        {
            const int complaintId = 80;
            const int creatorUserId = 15;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(complaintId, MiscEnumEntity.ComplaintApprovalRejected);

            await CreateSut().Consume(ctx.Object);

            var rejection = captured.Single(n =>
                n.ModuleName == MiscEnumEntity.NotifModuleComplaintRejected);
            rejection.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { creatorUserId });
            rejection.ModuleTransactionId.Should().Be(complaintId);
        }

        [Fact]
        public async Task Consume_QcReviewApproved_PublishesToMoChain()
        {
            const int complaintId = 90;
            const int creatorUserId = 16;
            const int moUserId = 37;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });
            _complaintQueryRepo.Setup(r => r.GetComplaintAgentIdsAsync(complaintId))
                .ReturnsAsync(new List<int> { 34 });
            _officerLookup.Setup(l => l.GetMarketingOfficerChainByAgentIdsAsync(
                    It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MoChainRow> { new() { AgentId = 34, MoUserId = moUserId } });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(
                complaintId,
                MiscEnumEntity.ComplaintApprovalApproved,
                MiscEnumEntity.ComplaintQCReviewModuleTypeName);

            await CreateSut().Consume(ctx.Object);

            var approval = captured.Single(n =>
                n.ModuleName == MiscEnumEntity.NotifModuleQcReviewApproved);
            approval.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { moUserId });
        }

        [Fact]
        public async Task Consume_QcReviewRejected_PublishesToReviewer()
        {
            const int complaintId = 91;
            const int creatorUserId = 17;
            const int reviewerUserId = 379;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });
            _qcReviewQueryRepo.Setup(r => r.GetByComplaintIdAsync(complaintId))
                .ReturnsAsync(new ComplaintQCReviewDto { ReviewedBy = reviewerUserId });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(
                complaintId,
                MiscEnumEntity.ComplaintApprovalRejected,
                MiscEnumEntity.ComplaintQCReviewModuleTypeName);

            await CreateSut().Consume(ctx.Object);

            var rejection = captured.Single(n =>
                n.ModuleName == MiscEnumEntity.NotifModuleQcReviewRejected);
            rejection.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { reviewerUserId });
        }

        [Fact]
        public async Task Consume_ResolutionRejected_PublishesToResolver()
        {
            const int complaintId = 92;
            const int creatorUserId = 18;
            const int resolverUserId = 2443;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 5, UnitName = "Unit-A" } });
            _resolutionQueryRepo.Setup(r => r.GetByComplaintHeaderIdAsync(complaintId))
                .ReturnsAsync(new ComplaintResolutionDto { ResolvedBy = resolverUserId });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(
                complaintId,
                MiscEnumEntity.ComplaintApprovalRejected,
                MiscEnumEntity.ComplaintResolutionModuleTypeName);

            await CreateSut().Consume(ctx.Object);

            var rejection = captured.Single(n =>
                n.ModuleName == MiscEnumEntity.NotifModuleResolutionRejected);
            rejection.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { resolverUserId });
        }

        [Fact]
        public async Task Consume_ResolutionApproved_PublishesToComplaintCreator()
        {
            // End-of-loop: Resolution approved -> notify the original complaint creator.
            const int complaintId = 93;
            const int creatorUserId = 19;

            _complaintQueryRepo.Setup(r => r.GetByIdAsync(complaintId))
                .ReturnsAsync(new ComplaintHeaderDto { Id = complaintId, CreatedBy = creatorUserId });
            _unitLookup.Setup(l => l.GetUserUnitAsync(creatorUserId))
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 7, UnitName = "Unit-B" } });

            StubNotificationEventTypeFound();

            var (ctx, captured) = BuildContext(
                complaintId,
                MiscEnumEntity.ComplaintApprovalApproved,
                MiscEnumEntity.ComplaintResolutionModuleTypeName);

            await CreateSut().Consume(ctx.Object);

            var approval = captured.Single(n =>
                n.ModuleName == MiscEnumEntity.NotifModuleResolutionApproved);
            approval.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { creatorUserId });
            approval.ModuleTransactionId.Should().Be(complaintId);
            approval.UnitId.Should().Be(7);
        }
    }
}
