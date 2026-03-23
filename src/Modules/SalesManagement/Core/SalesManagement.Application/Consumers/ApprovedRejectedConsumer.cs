using Contracts.Commands.Sales;
using Contracts.Events.Sales;
using MassTransit;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedSalesCommand>
    {
        private readonly IInvoiceCommandRepository _invoiceCommandRepo;
        private readonly IStoHeaderCommandRepository _stoHeaderCommandRepo;
        private readonly IDeliveryChallanCommandRepository _dcCommandRepo;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IInvoiceCommandRepository invoiceCommandRepo,
            IStoHeaderCommandRepository stoHeaderCommandRepo,
            IDeliveryChallanCommandRepository dcCommandRepo,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _invoiceCommandRepo = invoiceCommandRepo;
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
                // Route by ModuleTypeName — add more cases as more Sales pages get workflow
                switch (msg.ModuleTypeName)
                {
                    case MiscEnumEntity.TransactionTypeInvoice:
                        await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
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
    }
}
