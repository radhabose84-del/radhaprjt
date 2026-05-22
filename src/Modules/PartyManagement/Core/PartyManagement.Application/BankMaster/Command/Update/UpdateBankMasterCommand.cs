using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Update;
public record UpdateBankMasterCommand(UpdateBankMasterDto Dto) : IRequest, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}