using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;

public sealed class UpdatePurchaseBillEntryCommand : IRequest<Unit>, IRequirePermission
{
    public PurchaseBillEntryHeaderDto Data { get; set; } = default!;
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
