using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqTransporters
{
    public class GetFreightRfqTransportersQueryHandler
        : IRequestHandler<GetFreightRfqTransportersQuery, IReadOnlyList<TransporterLookupDto>>
    {
        private readonly ITransporterLookup _transporterLookup;

        public GetFreightRfqTransportersQueryHandler(ITransporterLookup transporterLookup)
        {
            _transporterLookup = transporterLookup;
        }

        public async Task<IReadOnlyList<TransporterLookupDto>> Handle(GetFreightRfqTransportersQuery request, CancellationToken cancellationToken)
        {
            return await _transporterLookup.SearchTransportersAsync(request.Term, cancellationToken);
        }
    }
}
