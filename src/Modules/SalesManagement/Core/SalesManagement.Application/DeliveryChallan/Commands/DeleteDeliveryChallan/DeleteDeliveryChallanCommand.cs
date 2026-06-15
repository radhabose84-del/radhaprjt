using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan
{
    public sealed record DeleteDeliveryChallanCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
