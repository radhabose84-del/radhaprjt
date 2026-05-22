using Contracts.Common;
using MediatR;
namespace PartyManagement.Application.BankMaster.Command.Create;
public record CreateBankMasterCommand(CreateBankMasterDto Dto) : IRequest<int>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}