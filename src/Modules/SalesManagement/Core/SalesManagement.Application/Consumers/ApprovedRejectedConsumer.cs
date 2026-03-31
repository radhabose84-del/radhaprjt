using System.Text.Json;
using Contracts.Commands.Finance;
using Contracts.Commands.Sales;
using Contracts.Events.Sales;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedSalesCommand>
    {
        private readonly IInvoiceCommandRepository _invoiceCommandRepo;
        private readonly ISalesOrderCommandRepository _salesOrderCommandRepo;
        private readonly ISalesOrderAmendmentCommandRepository _amendmentCommandRepo;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IStoHeaderCommandRepository _stoHeaderCommandRepo;
        private readonly IDeliveryChallanCommandRepository _dcCommandRepo;
        private readonly IComplaintCommandRepository _complaintCommandRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IInvoiceCommandRepository invoiceCommandRepo,
            ISalesOrderCommandRepository salesOrderCommandRepo,
            ISalesOrderAmendmentCommandRepository amendmentCommandRepo,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IStoHeaderCommandRepository stoHeaderCommandRepo,
            IDeliveryChallanCommandRepository dcCommandRepo,
            IComplaintCommandRepository complaintCommandRepo,
            IMediator mediator,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _invoiceCommandRepo = invoiceCommandRepo;
            _salesOrderCommandRepo = salesOrderCommandRepo;
            _amendmentCommandRepo = amendmentCommandRepo;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _stoHeaderCommandRepo = stoHeaderCommandRepo;
            _dcCommandRepo = dcCommandRepo;
            _complaintCommandRepo = complaintCommandRepo;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedSalesCommand> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "ApprovedRejectedSalesConsumer: ModuleTypeName={ModuleType}, Status={Status}, TransactionId={Id}",
                msg.ModuleTypeName, msg.Status, msg.ModuleTransactionId);

            try
            {
                // Route by ModuleTypeName
                switch (msg.ModuleTypeName)
                {
                    case MiscEnumEntity.TransactionTypeInvoice:
                        await HandleInvoiceApprovalAsync(msg, msg.DynamicFields, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesOrder:
                        await HandleSalesOrderApprovalAsync(msg, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesOrderAmendment:
                        await HandleSalesOrderAmendmentApprovalAsync(msg, context.CancellationToken);
                        break;
                    case MiscEnumEntity.StoModuleTypeName:
                        await _stoHeaderCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        _logger.LogInformation(
                            "STO {Id} status updated to {Status}",
                            msg.ModuleTransactionId, msg.Status);
                        break;

                    case MiscEnumEntity.DCModuleTypeName:
                        await _dcCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        _logger.LogInformation(
                            "DeliveryChallan {Id} status updated to {Status}",
                            msg.ModuleTransactionId, msg.Status);
                        break;

                    case MiscEnumEntity.ComplaintModuleTypeName:
                        await _complaintCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint {Id} status updated to {Status}",
                            msg.ModuleTransactionId, msg.Status);
                        break;

                    case MiscEnumEntity.ComplaintQCReviewModuleTypeName:
                        await _complaintCommandRepo.UpdateQCReviewApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint QC Review {Id} approval status updated to {Status}",
                            msg.ModuleTransactionId, msg.Status);
                        break;

                    case MiscEnumEntity.ComplaintResolutionModuleTypeName:
                        await _complaintCommandRepo.UpdateResolutionApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint Resolution {Id} approval status updated to {Status}",
                            msg.ModuleTransactionId, msg.Status);
                        break;

                    default:
                        _logger.LogWarning("Unknown Sales ModuleTypeName: {Type}", msg.ModuleTypeName);
                        return;
                }

                // Publish completion event
                await context.Publish(new ApprovedRejectedSalesCompletedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    ModuleTransactionId = msg.ModuleTransactionId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ApprovedRejectedSalesConsumer failed for TransactionId={Id}",
                    msg.ModuleTransactionId);
                throw; // MassTransit retry policy handles retries
            }
        }

        private async Task HandleSalesOrderAmendmentApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg, CancellationToken ct)
        {
            _logger.LogInformation(
                "SalesOrderAmendment Approval: Id={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            if (status != MiscEnumEntity.SalesOrderStatusApproved &&
                status != MiscEnumEntity.SalesOrderStatusRejected)
                return;

            var result = await _amendmentCommandRepo.ApplyAmendmentAsync(
                msg.ModuleTransactionId, status, ct);

            if (!result)
            {
                _logger.LogWarning(
                    "SalesOrderAmendment not found or already processed: Id={Id}",
                    msg.ModuleTransactionId);
                return;
            }

            _logger.LogInformation(
                "SalesOrderAmendment Id={Id} status updated to {Status}",
                msg.ModuleTransactionId, status);
        }

        private async Task HandleInvoiceApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg,
            List<JsonElement> dynamicFields,
            CancellationToken ct)
        {
            // 1. Always update Invoice approval status
            await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                msg.ModuleTransactionId, msg.Status, ct);

            // 2. Only proceed with EInvoice on Approval (not Rejection)
            if (msg.Status != MiscEnumEntity.InvoiceStatusApproved)
                return;

            // 3. Parse DynamicFields for withInvoice / withEwaybill flags
            bool withInvoice = ReadBool(dynamicFields, "withInvoice");
            bool withEwaybill = ReadBool(dynamicFields, "withEwaybill");

            if (!withInvoice)
            {
                _logger.LogInformation(
                    "Invoice {Id} approved. withInvoice=false, skipping EInvoice creation.",
                    msg.ModuleTransactionId);
                return;
            }

            // 4. Create EInvoice via Finance module (Contracts command → Finance handler)
            try
            {
                var command = new CreateEInvoiceFromSalesCommand
                {
                    InvoiceId = msg.ModuleTransactionId,
                    IsEwaybillCreate = withEwaybill
                };

                var result = await _mediator.Send(command, ct);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(
                        $"EInvoice creation failed for Invoice {msg.ModuleTransactionId}: {result.Message}");
                }

                _logger.LogInformation(
                    "EInvoice created for Invoice {InvoiceId}: EInvoiceHeaderId={EInvoiceId}, IRN={Irn}, EwbNo={EwbNo}",
                    msg.ModuleTransactionId,
                    result.Data?.EInvoiceHeaderId,
                    result.Data?.Irn,
                    result.Data?.EwbNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "EInvoice creation failed for Invoice {Id}, rolling back to Pending",
                    msg.ModuleTransactionId);

                await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                    msg.ModuleTransactionId,
                    MiscEnumEntity.InvoiceStatusPending,
                    ct);
            }
        }

        private async Task HandleSalesOrderApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg, CancellationToken ct)
        {
            _logger.LogInformation(
                "SalesOrder Consumer Approval Status Update: ModuleTransactionId={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            var salesOrderId = msg.ModuleTransactionId;

            if (msg.ModuleTypeName != MiscEnumEntity.TransactionTypeSalesOrder)
                return;

            // Resolve Approved and Rejected MiscMaster Ids
            var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusApproved);

            var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusRejected);

            if (status != MiscEnumEntity.SalesOrderStatusApproved && status != MiscEnumEntity.SalesOrderStatusRejected)
                return;

            var finalStatusId = status == MiscEnumEntity.SalesOrderStatusApproved
                ? statusApproved?.Id ?? 0
                : statusRejected?.Id ?? 0;

            var order = await _salesOrderCommandRepo.GetByIdEntityAsync(salesOrderId);
            if (order == null)
            {
                _logger.LogWarning("Sales Order not found for Id={SalesOrderId}", salesOrderId);
                return;
            }

            // Update status
            order.StatusId = finalStatusId;

            await _salesOrderCommandRepo.FinalizeOrderStatusAsync(order);

            _logger.LogInformation(
                "SalesOrder Id={SalesOrderId} status updated to {Status} (StatusId={StatusId})",
                salesOrderId, status, finalStatusId);
        }

        /// <summary>
        /// Reads a boolean value from the first DynamicFields JsonElement that contains the property.
        /// Expected format: [{ "withInvoice": true, "withEwaybill": false }]
        /// </summary>
        private static bool ReadBool(List<JsonElement> dynamicFields, string propertyName)
        {
            foreach (var element in dynamicFields)
            {
                if (element.ValueKind == JsonValueKind.Object &&
                    element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.True)
                        return true;
                    if (prop.ValueKind == JsonValueKind.False)
                        return false;
                    // Handle string "true"/"false"
                    if (prop.ValueKind == JsonValueKind.String &&
                        bool.TryParse(prop.GetString(), out var parsed))
                        return parsed;
                }
            }
            return false;
        }
    }
}
