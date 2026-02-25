using Contracts.Commands.Purchase;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using MassTransit;

namespace PurchaseManagement.Application.Consumers
{
    public class RollbackTransactionConsumer : IConsumer<RollbackTransactionCommand>
    {
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        public RollbackTransactionConsumer(IPurchaseIndentCommand purchaseIndentCommand)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
        }
        public async Task Consume(ConsumeContext<RollbackTransactionCommand> context)
        {
            await _purchaseIndentCommand.RollbackStatusAsync(context.Message.ModuleTransactionId);
        }
    }
}