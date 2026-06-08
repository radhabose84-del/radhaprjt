using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationBarcodeSeries
{
    // Term => dropdown list of series with balance. SeriesId => just that one series (Edit mode), even if fully allocated.
    public sealed record GetAllocationBarcodeSeriesQuery(string? Term, int? SeriesId = null) : IRequest<IReadOnlyList<BarcodeAllocationSeriesDto>>;
}
