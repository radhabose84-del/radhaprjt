using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;

public sealed class UpdatePurchaseBillEntryCommand : IRequest<Unit>
{
    public PurchaseBillEntryHeaderDto Data { get; set; } = default!;
}
