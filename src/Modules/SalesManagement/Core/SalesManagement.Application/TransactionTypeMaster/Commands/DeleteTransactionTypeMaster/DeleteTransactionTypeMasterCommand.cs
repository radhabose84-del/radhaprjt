using MediatR;

namespace SalesManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster
{
    public sealed record DeleteTransactionTypeMasterCommand(int Id) : IRequest<bool>;
}
