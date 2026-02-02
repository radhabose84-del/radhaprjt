using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;

public sealed class GetAllPurchaseBillEntryQuery 
    : IRequest<PurchaseBillEntryListVm>
{
    public int? VendorId { get; set; }
    public string? Search { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 15;
}

public sealed class PurchaseBillEntryListVm
{
    public int Total { get; set; }
    public List<PurchaseBillEntryHeaderDto> Items { get; set; } = new();
}
