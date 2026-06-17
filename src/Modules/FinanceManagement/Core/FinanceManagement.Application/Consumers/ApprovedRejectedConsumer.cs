using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Consumers
{
    // Applies a Finance approval result, routed by ModuleTypeName:
    //   • Account Group Move  → on Approved re-parent the group; on Rejected drop the request.
    //   • Tax Account Linkage → on Approved activate the PENDING row (Approved + IsActive=1) and
    //                           close the prior active row's EffectiveTo; on Rejected mark it Rejected.
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedFinanceCommand>
    {
        private readonly IAccountGroupChangeRequestRepository _changeRequestRepository;
        private readonly IAccountGroupCommandRepository _accountGroupCommandRepository;
        private readonly ITaxCodeCommandRepository _taxCodeCommandRepository;
        private readonly ITaxCodeQueryRepository _taxCodeQueryRepository;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IAccountGroupChangeRequestRepository changeRequestRepository,
            IAccountGroupCommandRepository accountGroupCommandRepository,
            ITaxCodeCommandRepository taxCodeCommandRepository,
            ITaxCodeQueryRepository taxCodeQueryRepository,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _changeRequestRepository = changeRequestRepository;
            _accountGroupCommandRepository = accountGroupCommandRepository;
            _taxCodeCommandRepository = taxCodeCommandRepository;
            _taxCodeQueryRepository = taxCodeQueryRepository;
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
    }
}
