using Contracts.Commands.Sales;
using Contracts.Events.Sales;
using MassTransit;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedSalesCommand>
    {
        private readonly IInvoiceCommandRepository _invoiceCommandRepo;
        private readonly ISalesOrderCommandRepository _salesOrderCommandRepo;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IStoHeaderCommandRepository _stoHeaderCommandRepo;
        private readonly IDeliveryChallanCommandRepository _dcCommandRepo;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IInvoiceCommandRepository invoiceCommandRepo,
            ISalesOrderCommandRepository salesOrderCommandRepo,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IStoHeaderCommandRepository stoHeaderCommandRepo,
            IDeliveryChallanCommandRepository dcCommandRepo,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _invoiceCommandRepo = invoiceCommandRepo;
            _salesOrderCommandRepo = salesOrderCommandRepo;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _stoHeaderCommandRepo = stoHeaderCommandRepo;
            _dcCommandRepo = dcCommandRepo;
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
                        await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesOrder:
                        await HandleSalesOrderApprovalAsync(msg, context.CancellationToken);
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
                MiscEnumEntity.SalesOrderApprovalStatus,MiscEnumEntity.SalesOrderStatusApproved);

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
    }
}
