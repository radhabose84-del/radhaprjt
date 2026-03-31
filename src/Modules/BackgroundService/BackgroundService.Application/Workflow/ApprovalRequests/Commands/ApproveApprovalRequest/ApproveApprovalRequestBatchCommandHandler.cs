using System.Text.Json;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Domain.Common;
using Contracts.Commands.Finance;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestBatchCommandHandler
        : IRequestHandler<ApproveApprovalRequestBatchCommand, ApproveBatchResultDto>
    {
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;
        private readonly IApprovalRequestCommand _approvalRequestCommand;
        private readonly IMiscMasterQueryRepository _miscMasterQuery;
        private readonly ISalesInvoiceLookup _salesInvoiceLookup;
        private readonly ILogger<ApproveApprovalRequestBatchCommandHandler> _logger;

        // set true if you want to stop the batch at first failure
        private const bool FailFast = true;

        public ApproveApprovalRequestBatchCommandHandler(
            IMediator mediator,
            IEventPublisher eventPublisher,
            IApprovalRequestCommand approvalRequestCommand,
            IMiscMasterQueryRepository miscMasterQuery,
            ISalesInvoiceLookup salesInvoiceLookup,
            ILogger<ApproveApprovalRequestBatchCommandHandler> logger)
        {
            _mediator = mediator;
            _eventPublisher = eventPublisher;
            _approvalRequestCommand = approvalRequestCommand;
            _miscMasterQuery = miscMasterQuery;
            _salesInvoiceLookup = salesInvoiceLookup;
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

            // ── Synchronous EInvoice + IRN creation for approved Invoices ──
            // This runs BEFORE the API response, so IRN errors appear in result.Errors.
            // The consumer will also attempt creation, but the idempotent handler prevents duplicates.
            foreach (var item in request.Items)
            {
                if (item == null || item.IsApproved != 1)
                    continue;

                bool withInvoice = ReadBool(item.DynamicFields, "withInvoice");
                if (!withInvoice)
                    continue;

                bool withEwaybill = ReadBool(item.DynamicFields, "withEwaybill");

                try
                {
                    var eInvoiceResult = await _mediator.Send(new CreateEInvoiceFromSalesCommand
                    {
                        InvoiceId = item.ModuleTransactionId,
                        IsEwaybillCreate = withEwaybill
                    }, cancellationToken);

                    if (!eInvoiceResult.IsSuccess)
                    {
                        result.Errors.Add(
                            $"Invoice {item.ModuleTransactionId}: EInvoice/IRN failed - {eInvoiceResult.Message}");
                        _logger.LogWarning(
                            "EInvoice/IRN failed for Invoice {InvoiceId}: {Message}",
                            item.ModuleTransactionId, eInvoiceResult.Message);

                        await RevertAllStatusesAsync(
                            item.ApprovalRequestHeaderId, item.ModuleTransactionId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "EInvoice/IRN created for Invoice {InvoiceId}, IRN={Irn}",
                            item.ModuleTransactionId, eInvoiceResult.Data?.Irn);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(
                        $"Invoice {item.ModuleTransactionId}: EInvoice creation error - {ex.Message}");
                    _logger.LogError(ex,
                        "EInvoice creation error for Invoice {InvoiceId}",
                        item.ModuleTransactionId);

                    await RevertAllStatusesAsync(
                        item.ApprovalRequestHeaderId, item.ModuleTransactionId, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Batch Approve Completed | Total={Total} Processed={Processed} Approved={Approved} Rejected={Rejected} Failed={Failed} Skipped={Skipped}",
                result.Total, result.ProcessedCount, result.ApprovedCount, result.RejectedCount, result.FailedCount, result.SkippedCount);

            return result;
        }

        private async Task RevertAllStatusesAsync(int approvalRequestHeaderId, int invoiceId, CancellationToken ct)
        {
            // 1. Revert AppData.ApprovalRequest StatusId → Pending
            try
            {
                var pendingStatus = await _miscMasterQuery.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
                if (pendingStatus == null)
                {
                    _logger.LogError(
                        "Cannot revert ApprovalRequest {HeaderId}: MiscMaster 'ApprovalStatus/Pending' not found",
                        approvalRequestHeaderId);
                }
                else
                {
                    await _approvalRequestCommand.RevertStatusAsync(
                        approvalRequestHeaderId, pendingStatus.Id, ct);
                    _logger.LogInformation(
                        "ApprovalRequest {HeaderId} status reverted to Pending (StatusId={StatusId})",
                        approvalRequestHeaderId, pendingStatus.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to revert ApprovalRequest {HeaderId} status to Pending",
                    approvalRequestHeaderId);
            }

            // 2. Revert Sales.InvoiceHeader StatusId → Pending
            try
            {
                await _salesInvoiceLookup.RevertInvoiceStatusToPendingAsync(invoiceId, ct);
                _logger.LogInformation(
                    "Sales.InvoiceHeader {InvoiceId} status reverted to Pending",
                    invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to revert Sales.InvoiceHeader {InvoiceId} status to Pending",
                    invoiceId);
            }
        }

        private static bool ReadBool(List<JsonElement>? dynamicFields, string propertyName)
        {
            if (dynamicFields == null) return false;
            foreach (var element in dynamicFields)
            {
                if (element.ValueKind == JsonValueKind.Object &&
                    element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.True) return true;
                    if (prop.ValueKind == JsonValueKind.False) return false;
                    if (prop.ValueKind == JsonValueKind.String &&
                        bool.TryParse(prop.GetString(), out var parsed))
                        return parsed;
                }
            }
            return false;
        }
    }
}
