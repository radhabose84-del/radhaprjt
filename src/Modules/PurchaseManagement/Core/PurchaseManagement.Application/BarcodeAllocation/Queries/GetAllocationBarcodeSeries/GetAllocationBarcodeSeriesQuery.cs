using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationBarcodeSeries
{
    public sealed record GetAllocationBarcodeSeriesQuery(string? Term) : IRequest<IReadOnlyList<BarcodeAllocationSeriesDto>>;
}
