using MediatR;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPoPrefill
{
    /// <summary>Returns the PO-driven prefill values when a PO Reference is selected.</summary>
    public sealed record GetFreightRfqPoPrefillQuery(int PoId) : IRequest<FreightRfqPoPrefillDto?>;
}
