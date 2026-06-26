using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Consumers
{
    // Applies a Finance approval result, routed by ModuleTypeName:
    //   • Account Group Move  → on Approved re-parent the group; on Rejected drop the request.
    //   • Tax Account Linkage → on Approved activate the PENDING row (Approved + IsActive=1) and
    //                           close the prior active row's EffectiveTo; on Rejected mark it Rejected.
    //   • Journal Voucher     → on Approved set status APPROVED; a MANUAL voucher is then posted immediately
    //                           (updates ledger balances). On Rejected set status REJECTED.
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedFinanceCommand>
    {
        private readonly IAccountGroupChangeRequestRepository _changeRequestRepository;
        private readonly IAccountGroupCommandRepository _accountGroupCommandRepository;
        private readonly ITaxCodeCommandRepository _taxCodeCommandRepository;
        private readonly ITaxCodeQueryRepository _taxCodeQueryRepository;
        private readonly IJournalCommandRepository _journalCommandRepository;
        private readonly IJournalQueryRepository _journalQueryRepository;
        private readonly IRecurringJournalTemplateCommandRepository _recurringTemplateCommandRepository;
        private readonly IRecurringJournalTemplateQueryRepository _recurringTemplateQueryRepository;
        private readonly IRecurringTemplateScheduler _recurringTemplateScheduler;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IAccountGroupChangeRequestRepository changeRequestRepository,
            IAccountGroupCommandRepository accountGroupCommandRepository,
            ITaxCodeCommandRepository taxCodeCommandRepository,
            ITaxCodeQueryRepository taxCodeQueryRepository,
            IJournalCommandRepository journalCommandRepository,
            IJournalQueryRepository journalQueryRepository,
            IRecurringJournalTemplateCommandRepository recurringTemplateCommandRepository,
            IRecurringJournalTemplateQueryRepository recurringTemplateQueryRepository,
            IRecurringTemplateScheduler recurringTemplateScheduler,
            ITimeZoneService timeZoneService,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _changeRequestRepository = changeRequestRepository;
            _accountGroupCommandRepository = accountGroupCommandRepository;
            _taxCodeCommandRepository = taxCodeCommandRepository;
            _taxCodeQueryRepository = taxCodeQueryRepository;
            _journalCommandRepository = journalCommandRepository;
            _journalQueryRepository = journalQueryRepository;
            _recurringTemplateCommandRepository = recurringTemplateCommandRepository;
            _recurringTemplateQueryRepository = recurringTemplateQueryRepository;
            _recurringTemplateScheduler = recurringTemplateScheduler;
            _timeZoneService = timeZoneService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedFinanceCommand> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation("Finance approval result: {@Message}", msg);

            var isApproved = string.Equals(msg.Status, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase);
            var isRejected = string.Equals(msg.Status, MiscEnumEntity.Rejected, StringComparison.OrdinalIgnoreCase);
            if (!isApproved && !isRejected)
                return; // pending / unknown — nothing to apply

            switch (msg.ModuleTypeName)
            {
                case MiscEnumEntity.AccountGroupHierarchy:
                    await HandleAccountGroupAsync(msg, isApproved, ct);
                    break;

                case MiscEnumEntity.TaxAccountLinkage:
                    await HandleTaxAccountLinkageAsync(msg, isApproved, ct);
                    break;

                case MiscEnumEntity.JournalVoucher:
                    await HandleJournalAsync(msg, isApproved, ct);
                    break;

                case MiscEnumEntity.RecurringJournalTemplate:
                    await HandleRecurringTemplateAsync(msg, isApproved, ct);
                    break;

                default:
                    _logger.LogWarning("Unknown Finance ModuleTypeName: {Type}", msg.ModuleTypeName);
                    return;
            }
        }

        private async Task HandleAccountGroupAsync(UpdateApprovedRejectedFinanceCommand msg, bool isApproved, CancellationToken ct)
        {
            // ModuleTransactionId carries the AccountGroupId being moved.
            var changeRequest = await _changeRequestRepository.GetPendingByAccountGroupAsync(msg.ModuleTransactionId, ct);
            if (changeRequest == null)
            {
                _logger.LogWarning("No pending Account Group change request for AccountGroupId {Id}", msg.ModuleTransactionId);
                return;
            }

            if (isApproved)
            {
                await _accountGroupCommandRepository.MoveAsync(changeRequest.AccountGroupId, changeRequest.NewParentAccountGroupId);
                await _changeRequestRepository.MarkStatusAsync(changeRequest.Id, MiscEnumEntity.Approved, ct);
                _logger.LogInformation("Account Group {GroupId} moved under {ParentId} (change request {CrId} approved).",
                    changeRequest.AccountGroupId, changeRequest.NewParentAccountGroupId, changeRequest.Id);
            }
            else
            {
                await _changeRequestRepository.MarkStatusAsync(changeRequest.Id, MiscEnumEntity.Rejected, ct);
                _logger.LogInformation("Account Group move request {CrId} rejected.", changeRequest.Id);
            }
        }

        private async Task HandleTaxAccountLinkageAsync(UpdateApprovedRejectedFinanceCommand msg, bool isApproved, CancellationToken ct)
        {
            // ModuleTransactionId carries the PENDING TaxAccountLinkage row Id.
            if (isApproved)
            {
                var approvedStatusId = await _taxCodeQueryRepository.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved)
                    ?? throw new InvalidOperationException("ApprovalStatus 'Approved' is not configured in MiscMaster.");

                // Activates the PENDING row (Approved + IsActive=1) AND closes the prior active row (IsActive=0 + EffectiveTo).
                var ok = await _taxCodeCommandRepository.ActivateLinkageAsync(msg.ModuleTransactionId, approvedStatusId, ct);
                if (!ok)
                {
                    _logger.LogWarning("Tax-account linkage {Id} not found for approval.", msg.ModuleTransactionId);
                    return;
                }

                _logger.LogInformation(
                    "Tax-account linkage {Id} approved → activated and prior active row closed.", msg.ModuleTransactionId);
            }
            else
            {
                var rejectedStatusId = await _taxCodeQueryRepository.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected)
                    ?? throw new InvalidOperationException("ApprovalStatus 'Rejected' is not configured in MiscMaster.");

                var ok = await _taxCodeCommandRepository.RejectLinkageAsync(msg.ModuleTransactionId, rejectedStatusId, ct);
                if (!ok)
                {
                    _logger.LogWarning("Tax-account linkage {Id} not found for rejection.", msg.ModuleTransactionId);
                    return;
                }

                _logger.LogInformation("Tax-account linkage {Id} rejected.", msg.ModuleTransactionId);
            }
        }

        private async Task HandleJournalAsync(UpdateApprovedRejectedFinanceCommand msg, bool isApproved, CancellationToken ct)
        {
            // ModuleTransactionId carries the JournalHeader.Id. Approved → APPROVED (now postable);
            // Rejected → REJECTED.
            var statusId = await _journalQueryRepository.GetStatusIdAsync(isApproved ? "APPROVED" : "REJECTED");
            var now = _timeZoneService.GetCurrentTime();

            var ok = await _journalCommandRepository.SetApprovalResultAsync(
                msg.ModuleTransactionId, statusId, isApproved, msg.ModifiedByName, now, ct);
            if (!ok)
            {
                _logger.LogWarning("Journal {Id} not found for approval result.", msg.ModuleTransactionId);
                return;
            }

            // A MANUAL voucher is posted immediately on approval (updates ledger balances). System-sourced
            // vouchers (RECURRING / IMPORT) stay APPROVED and are posted by their own flow / the postable list.
            if (isApproved)
            {
                var journal = await _journalQueryRepository.GetByIdAsync(msg.ModuleTransactionId);
                //var manualSourceId = await _journalQueryRepository.GetSourceIdAsync("MANUAL");
                if (journal != null )
                {
                    var postedStatusId = await _journalQueryRepository.GetStatusIdAsync("POSTED");
                    // fyName = null: a manual voucher already carries its number from create time.
                    await _journalCommandRepository.PostAsync(
                        msg.ModuleTransactionId, postedStatusId, null, msg.ModifiedByName, 0, now, ct);
                    _logger.LogInformation("Journal {Id} approved → posted (manual).", msg.ModuleTransactionId);
                    return;
                }
            }

            _logger.LogInformation("Journal {Id} {Result}.",
                msg.ModuleTransactionId, isApproved ? "approved" : "rejected → returned to draft");
        }

        private async Task HandleRecurringTemplateAsync(UpdateApprovedRejectedFinanceCommand msg, bool isApproved, CancellationToken ct)
        {
            // ModuleTransactionId carries the RecurringJournalTemplateHeader.Id. Approved → status Approved +
            // (re)schedule the auto-post Hangfire job; Rejected → status Rejected + remove any job.
            var statusId = await _recurringTemplateQueryRepository.GetApprovalStatusIdAsync(
                isApproved ? MiscEnumEntity.Approved : MiscEnumEntity.Rejected);

            var ok = await _recurringTemplateCommandRepository.SetApprovalResultAsync(msg.ModuleTransactionId, statusId, ct);
            if (!ok)
            {
                _logger.LogWarning("Recurring template {Id} not found for approval result.", msg.ModuleTransactionId);
                return;
            }

            if (isApproved)
                await _recurringTemplateScheduler.SyncAsync(msg.ModuleTransactionId, ct);   // schedules only if AutoPost
            else
                _recurringTemplateScheduler.Remove(msg.ModuleTransactionId);

            _logger.LogInformation("Recurring template {Id} {Result}.",
                msg.ModuleTransactionId, isApproved ? "approved → scheduled" : "rejected");
        }
    }
}
