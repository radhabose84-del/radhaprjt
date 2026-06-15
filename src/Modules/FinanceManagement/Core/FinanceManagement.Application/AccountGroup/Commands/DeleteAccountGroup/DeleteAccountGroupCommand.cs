using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup
{
    public sealed record DeleteAccountGroupCommand(int Id) : IRequest<bool>;
}
