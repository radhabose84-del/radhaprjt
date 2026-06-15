using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Delete
{
    public record DeleteDutyMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}