using PurchaseManagement.Application.Port.Dto;
using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands;
public sealed record CreatePortMasterCommand(
    string PortCode, string PortName, int CountryId,  int PortTypeId
) : IRequest<PortMasterDto>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}
