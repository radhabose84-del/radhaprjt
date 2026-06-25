using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesLabels
{
    /// <summary>Builds the print-ready bale-barcode label payload for a whole barcode series.</summary>
    public sealed record GetBarcodeSeriesLabelsQuery(int Id) : IRequest<BarcodeLabelReportDto?>;
}
