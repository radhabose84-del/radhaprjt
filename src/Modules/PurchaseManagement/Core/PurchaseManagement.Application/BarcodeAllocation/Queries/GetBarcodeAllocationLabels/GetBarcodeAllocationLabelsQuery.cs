using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationLabels
{
    /// <summary>Builds the print-ready bale-barcode label payload for one allocation.</summary>
    public sealed record GetBarcodeAllocationLabelsQuery(int Id) : IRequest<BarcodeLabelReportDto?>;
}
