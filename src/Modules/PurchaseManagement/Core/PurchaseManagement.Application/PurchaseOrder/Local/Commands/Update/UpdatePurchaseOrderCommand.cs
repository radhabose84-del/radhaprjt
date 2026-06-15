// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
public record UpdatePurchaseOrderCommand : IRequest<bool>, IRequirePermission
{
    public required PurchaseOrderUpdateDto Data { get; init; }
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}

