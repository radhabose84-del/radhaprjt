using Contracts.Common;
using MediatR;
namespace PurchaseManagement.Application.Port.Commands;
public sealed record DeletePortMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
