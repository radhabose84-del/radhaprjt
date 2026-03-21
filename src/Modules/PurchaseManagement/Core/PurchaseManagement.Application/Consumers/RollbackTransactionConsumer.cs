using Contracts.Commands.Purchase;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.Consumers
{
    public class RollbackTransactionConsumer : IConsumer<RollbackTransactionCommand>
    {
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        private readonly IPriceMasterCommandRepository _priceMasterCommand;
        private readonly IRfqCommandRepository _rfqCommand;
        private readonly ILogger<RollbackTransactionConsumer> _logger;

        public RollbackTransactionConsumer(
            IPurchaseIndentCommand purchaseIndentCommand,
            IPriceMasterCommandRepository priceMasterCommand,
            IRfqCommandRepository rfqCommand,
            ILogger<RollbackTransactionConsumer> logger)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _priceMasterCommand = priceMasterCommand;
            _rfqCommand = rfqCommand;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RollbackTransactionCommand> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "RollbackTransactionConsumer received: ModuleTypeName={ModuleTypeName}, ModuleTransactionId={Id}, Reason={Reason}",
                msg.ModuleTypeName, msg.ModuleTransactionId, msg.Reason);

            if (msg.ModuleTypeName == MiscEnumEntity.PriceMaster)
            {
                var result = await _priceMasterCommand.RollbackStatusAsync(
                    msg.ModuleTransactionId, context.CancellationToken);

                if (!result)
                    _logger.LogWarning("PriceMaster rollback failed — Id {Id} not found or already deleted.", msg.ModuleTransactionId);

                return;
            }

            if (msg.ModuleTypeName == MiscEnumEntity.RFQ)
            {
                var result = await _rfqCommand.RollbackStatusAsync(
                    msg.ModuleTransactionId, context.CancellationToken);

                if (!result)
                    _logger.LogWarning("RFQ rollback failed — Id {Id} not found or already deleted.", msg.ModuleTransactionId);

                return;
            }

            // Default: PurchaseIndent rollback (existing behavior)
            await _purchaseIndentCommand.RollbackStatusAsync(msg.ModuleTransactionId);
        }
    }
}
