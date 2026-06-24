using MediatR;
using PurchaseManagement.Application.Common.Dtos;

namespace PurchaseManagement.Application.OCREntry.Queries.GetNextOcrNumber
{
    /// <summary>Returns the last-issued and next OCR number for the OCR Management screen (peek, no increment).</summary>
    public sealed record GetNextOcrNumberQuery : IRequest<DocumentNumberPeekDto>;
}
