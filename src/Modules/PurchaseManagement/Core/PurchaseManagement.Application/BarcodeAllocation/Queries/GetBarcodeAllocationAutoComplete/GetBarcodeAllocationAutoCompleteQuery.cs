using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationAutoComplete
{
    public sealed record GetBarcodeAllocationAutoCompleteQuery(string? Term) : IRequest<IReadOnlyList<BarcodeAllocationLookupDto>>;
}
