using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster
{
    public sealed record DeleteTransactionTypeMasterCommand(int Id) : IRequest<bool>;
}
