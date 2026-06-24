using MediatR;
using PurchaseManagement.Application.Common.Dtos;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetNextRawMaterialPONumber
{
    /// <summary>Returns the last-issued and next Raw Material PO number for the OCR→PO conversion screen (peek, no increment).</summary>
    public sealed record GetNextRawMaterialPONumberQuery : IRequest<DocumentNumberPeekDto>;
}
