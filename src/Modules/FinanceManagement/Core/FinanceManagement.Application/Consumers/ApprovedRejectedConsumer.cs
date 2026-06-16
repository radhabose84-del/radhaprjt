using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Consumers
{
    // Applies a Finance approval result. Currently handles Account Group Move:
    // on Approved → re-parent the group; on Rejected → drop the request.
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedFinanceCommand>
    {
        private readonly IAccountGroupChangeRequestRepository _changeRequestRepository;
        private readonly IAccountGroupCommandRepository _accountGroupCommandRepository;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IAccountGroupChangeRequestRepository changeRequestRepository,
            IAccountGroupCommandRepository accountGroupCommandRepository,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _changeRequestRepository = changeRequestRepository;
            _accountGroupCommandRepository = accountGroupCommandRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedFinanceCommand> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation("Finance approval result: {@Message}", msg);

            // Only Account Group Move is handled here for now.
            if (!string.Equals(msg.ModuleTypeName, MiscEnumEntity.AccountGroupHierarchy, StringComparison.OrdinalIgnoreCase))
                return;

            var isApproved = string.Equals(msg.Status, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase);
            var isRejected = string.Equals(msg.Status, MiscEnumEntity.Rejected, StringComparison.OrdinalIgnoreCase);
            if (!isApproved && !isRejected)
                return; // pending / unknown — nothing to apply

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
    }
}
