using BackgroundService.Application.Notification.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestBatchCommandHandler
        : IRequestHandler<ApproveApprovalRequestBatchCommand, ApproveBatchResultDto>
    {
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ApproveApprovalRequestBatchCommandHandler> _logger;

        // set true if you want to stop the batch at first failure
        private const bool FailFast = true;

        public ApproveApprovalRequestBatchCommandHandler(
            IMediator mediator,
            IEventPublisher eventPublisher,
            ILogger<ApproveApprovalRequestBatchCommandHandler> logger)
        {
            _mediator = mediator;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<ApproveBatchResultDto> Handle(
            ApproveApprovalRequestBatchCommand request,
            CancellationToken cancellationToken)
        {
            var result = new ApproveBatchResultDto();

            if (request?.Items == null || request.Items.Count == 0)
                return result;

            result.Total = request.Items.Count;

            foreach (var item in request.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item == null || item.ApprovalRequestHeaderId <= 0 || item.ModuleTransactionId <= 0)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Skipped invalid item. HeaderId={item?.ApprovalRequestHeaderId}, TxnId={item?.ModuleTransactionId}");
                    continue;
                }

                try
                {
                    _logger.LogInformation("Batch Approve -> HeaderId={HeaderId} TxnId={TxnId} IsApproved={IsApproved}",
                        item.ApprovalRequestHeaderId, item.ModuleTransactionId, item.IsApproved);

                    await _mediator.Send(new ApproveApprovalRequestCommand
                    {
                        ApprovalRequestHeaderId = item.ApprovalRequestHeaderId,
                        ModuleTransactionId = item.ModuleTransactionId,
                        Remark = item.Remark ?? string.Empty,
                        IsApproved = item.IsApproved,
                        ApprovalDocument = item.ApprovalDocument,
                        ApprovalRequestLine = item.ApprovalRequestLine,
                        PartyContacts = item.PartyContacts ?? new(),
                        DynamicFields = item.DynamicFields ?? new()
                    }, cancellationToken);

                    result.ProcessedCount++;

                    if (item.IsApproved == 1) result.ApprovedCount++;
                    else result.RejectedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed HeaderId={item.ApprovalRequestHeaderId}, TxnId={item.ModuleTransactionId}: {ex.Message}");

                    _logger.LogError(ex,
                        "Batch Approve failed for HeaderId={HeaderId} TxnId={TxnId}",
                        item.ApprovalRequestHeaderId, item.ModuleTransactionId);

                    if (FailFast)
                        throw; // stop immediately
                }
            }

            // ✅ publish all ApprovedRejectedEvent once (Outbox pattern)
            await _eventPublisher.PublishPendingEventsAsync();

            _logger.LogInformation(
                "Batch Approve Completed | Total={Total} Processed={Processed} Approved={Approved} Rejected={Rejected} Failed={Failed} Skipped={Skipped}",
                result.Total, result.ProcessedCount, result.ApprovedCount, result.RejectedCount, result.FailedCount, result.SkippedCount);

            return result;
        }
    }
}
