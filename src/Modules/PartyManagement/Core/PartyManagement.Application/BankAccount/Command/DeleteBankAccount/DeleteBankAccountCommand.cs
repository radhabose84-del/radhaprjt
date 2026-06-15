using Contracts.Common;
using MediatR;


namespace PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
public sealed record DeleteBankAccountCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}