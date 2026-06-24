using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Consumers;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.Consumers
{
    // The Finance ApprovedRejectedConsumer routes by ModuleTypeName:
    //   • Account Group Hierarchy → re-parent on approval (US-GL02-02)
    //   • Tax Account Linkage     → activate/reject the pending linkage (US-GL02-05B)
    //   • Journal Voucher         → approve → APPROVED, reject → back to DRAFT (US-GL01-06B)
    public sealed class ApprovedRejectedConsumerTests
    {
        private readonly Mock<IAccountGroupChangeRequestRepository> _changeRepo = new(MockBehavior.Loose);
        private readonly Mock<IAccountGroupCommandRepository> _agCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<ITaxCodeCommandRepository> _taxCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITaxCodeQueryRepository> _taxQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalCommandRepository> _journalCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IJournalQueryRepository> _journalQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _timeZone = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ApprovedRejectedConsumer>> _logger = new();

        private const int ApprovedStatusId = 48;
        private const int RejectedStatusId = 50;
        private const int LinkageId = 21;
        private const int JournalId = 77;
        private const int JournalApprovedStatusId = 100;
        private const int JournalRejectedStatusId = 102;

        private ApprovedRejectedConsumer CreateSut() =>
            new(_changeRepo.Object, _agCommandRepo.Object, _taxCommandRepo.Object, _taxQueryRepo.Object,
                _journalCommandRepo.Object, _journalQueryRepo.Object, _timeZone.Object, _logger.Object);

        private static Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>> BuildContext(UpdateApprovedRejectedFinanceCommand msg)
        {
            var ctx = new Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>>(MockBehavior.Loose);
            ctx.SetupGet(c => c.Message).Returns(msg);
            ctx.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
            return ctx;
        }

        // ── Account Group Move (US-GL02-02) ───────────────────────────────────────
        private static ConsumeContext<UpdateApprovedRejectedFinanceCommand> AgCtx(string moduleTypeName, string status, int moduleTransactionId = 3) =>
            BuildContext(new UpdateApprovedRejectedFinanceCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTypeName = moduleTypeName,
                ModuleTransactionId = moduleTransactionId,
                Status = status
            }).Object;

        private void SetupPendingChangeRequest() =>
            _changeRepo
                .Setup(r => r.GetPendingByAccountGroupAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountGroupChangeRequest { Id = 1, AccountGroupId = 3, NewParentAccountGroupId = 5 });

        [Fact]
        public async Task Consume_Approved_AppliesMoveAndMarksApproved()
        {
            SetupPendingChangeRequest();

            await CreateSut().Consume(AgCtx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Approved));

            _agCommandRepo.Verify(r => r.MoveAsync(3, 5), Times.Once);
            _changeRepo.Verify(r => r.MarkStatusAsync(1, MiscEnumEntity.Approved, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_Rejected_MarksRejectedAndDoesNotMove()
        {
            SetupPendingChangeRequest();

            await CreateSut().Consume(AgCtx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Rejected));

            _changeRepo.Verify(r => r.MarkStatusAsync(1, MiscEnumEntity.Rejected, It.IsAny<CancellationToken>()), Times.Once);
            _agCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Consume_OtherModuleType_Ignored()
        {
            await CreateSut().Consume(AgCtx("Purchase Order", MiscEnumEntity.Approved));

            _changeRepo.Verify(r => r.GetPendingByAccountGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _agCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Consume_NoPendingChangeRequest_DoesNothing()
        {
            _changeRepo
                .Setup(r => r.GetPendingByAccountGroupAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountGroupChangeRequest?)null);

            await CreateSut().Consume(AgCtx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Approved));

            _agCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _changeRepo.Verify(r => r.MarkStatusAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Consume_UnknownStatus_Ignored()
        {
            await CreateSut().Consume(AgCtx(MiscEnumEntity.AccountGroupHierarchy, "Pending"));

            _changeRepo.Verify(r => r.GetPendingByAccountGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _agCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        // ── Tax Account Linkage (US-GL02-05B) ─────────────────────────────────────
        private static Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>> TaxContext(string status) =>
            BuildContext(new UpdateApprovedRejectedFinanceCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTransactionId = LinkageId,
                ModuleTypeName = MiscEnumEntity.TaxAccountLinkage,
                Status = status
            });

        [Fact]
        public async Task Consume_TaxLinkageApproved_ActivatesLinkage()
        {
            _taxQueryRepo.Setup(r => r.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved)).ReturnsAsync(ApprovedStatusId);
            _taxCommandRepo.Setup(r => r.ActivateLinkageAsync(LinkageId, ApprovedStatusId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Consume(TaxContext(MiscEnumEntity.Approved).Object);

            _taxCommandRepo.Verify(r => r.ActivateLinkageAsync(LinkageId, ApprovedStatusId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_TaxLinkageRejected_RejectsLinkage()
        {
            _taxQueryRepo.Setup(r => r.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected)).ReturnsAsync(RejectedStatusId);
            _taxCommandRepo.Setup(r => r.RejectLinkageAsync(LinkageId, RejectedStatusId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Consume(TaxContext(MiscEnumEntity.Rejected).Object);

            _taxCommandRepo.Verify(r => r.RejectLinkageAsync(LinkageId, RejectedStatusId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_PendingStatus_DoesNothing()
        {
            // Strict tax mocks would throw if any repo were touched.
            await CreateSut().Consume(TaxContext(MiscEnumEntity.Pending).Object);
        }

        // ── Journal Voucher (US-GL01-06B) ─────────────────────────────────────────
        private static ConsumeContext<UpdateApprovedRejectedFinanceCommand> JournalCtx(string status) =>
            BuildContext(new UpdateApprovedRejectedFinanceCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTransactionId = JournalId,
                ModuleTypeName = MiscEnumEntity.JournalVoucher,
                Status = status,
                ModifiedByName = "Approver Bob"
            }).Object;

        [Fact]
        public async Task Consume_JournalApproved_SetsApprovedStatusWithApproverName()
        {
            _journalQueryRepo.Setup(r => r.GetStatusIdAsync("APPROVED")).ReturnsAsync(JournalApprovedStatusId);

            await CreateSut().Consume(JournalCtx(MiscEnumEntity.Approved));

            _journalCommandRepo.Verify(r => r.SetApprovalResultAsync(
                JournalId, JournalApprovedStatusId, true, "Approver Bob", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Consume_JournalRejected_SetsRejectedStatus()
        {
            _journalQueryRepo.Setup(r => r.GetStatusIdAsync("REJECTED")).ReturnsAsync(JournalRejectedStatusId);

            await CreateSut().Consume(JournalCtx(MiscEnumEntity.Rejected));

            _journalCommandRepo.Verify(r => r.SetApprovalResultAsync(
                JournalId, JournalRejectedStatusId, false, "Approver Bob", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
