using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Cancel;

public sealed record CancelPurchaseOrderCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
