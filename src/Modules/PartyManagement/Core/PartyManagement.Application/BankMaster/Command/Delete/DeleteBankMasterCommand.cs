using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Delete;

public record DeleteBankMasterCommand(int Id) : IRequest, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}

