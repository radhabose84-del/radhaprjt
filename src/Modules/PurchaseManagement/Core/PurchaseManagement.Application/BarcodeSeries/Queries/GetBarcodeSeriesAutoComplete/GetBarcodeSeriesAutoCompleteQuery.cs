using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesAutoComplete
{
    public sealed record GetBarcodeSeriesAutoCompleteQuery(string? Term) : IRequest<IReadOnlyList<BarcodeSeriesLookupDto>>;
}
