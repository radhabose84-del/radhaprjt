using MediatR;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetPendingPoReferences
{
    /// <summary>Pending Purchase Orders for the "PO Reference" dropdown (PO-based RFQ).</summary>
    public sealed record GetPendingPoReferencesQuery(string? Term) : IRequest<IReadOnlyList<PoReferenceLookupDto>>;
}
