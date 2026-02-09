using Contracts.Commands.Purchase;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using MassTransit;

namespace BudgetManagement.Application.Consumers
{
    public class RollbackTransactionConsumer : IConsumer<RollbackTransactionCommand>
    {
        private readonly IBudgetRequestCommandRepository _budgetRequestCommand;
        public RollbackTransactionConsumer(IBudgetRequestCommandRepository budgetRequestCommand)
        {
            _budgetRequestCommand = budgetRequestCommand;
        }
        public async Task Consume(ConsumeContext<RollbackTransactionCommand> context)
        {
            await _budgetRequestCommand.RollbackStatusAsync(context.Message.ModuleTransactionId);
        }
    }
}