using Contracts.Dtos.Lookups.Party;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqTransporters
{
    /// <summary>Transporter autocomplete for the Freight Comparison rows.</summary>
    public sealed record GetFreightRfqTransportersQuery(string? Term) : IRequest<IReadOnlyList<TransporterLookupDto>>;
}
