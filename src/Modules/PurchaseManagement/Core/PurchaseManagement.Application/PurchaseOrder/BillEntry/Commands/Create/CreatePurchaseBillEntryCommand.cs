using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;

  public sealed record CreatePurchaseBillEntryCommand(
        PurchaseBillEntryHeaderDto Data
    ) : IRequest<int>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}