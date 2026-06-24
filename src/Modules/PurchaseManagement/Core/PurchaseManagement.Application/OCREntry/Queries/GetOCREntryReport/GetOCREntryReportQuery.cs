using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryReport
{
    /// <summary>Builds the dynamic Order Confirmation Report payload for a single OCR.</summary>
    public sealed record GetOCREntryReportQuery(int Id) : IRequest<OcrReportDto?>;
}
