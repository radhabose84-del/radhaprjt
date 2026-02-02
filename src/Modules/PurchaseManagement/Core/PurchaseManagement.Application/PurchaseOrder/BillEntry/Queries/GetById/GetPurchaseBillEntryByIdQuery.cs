using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetById;

public sealed class GetPurchaseBillEntryByIdQuery 
    : IRequest<PurchaseBillEntryHeaderDto>
{
    public int Id { get; set; }
}
