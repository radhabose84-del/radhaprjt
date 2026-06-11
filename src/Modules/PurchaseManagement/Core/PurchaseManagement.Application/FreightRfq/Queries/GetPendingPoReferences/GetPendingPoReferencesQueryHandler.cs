using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetPendingPoReferences
{
    public class GetPendingPoReferencesQueryHandler
        : IRequestHandler<GetPendingPoReferencesQuery, IReadOnlyList<PoReferenceLookupDto>>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;

        public GetPendingPoReferencesQueryHandler(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<PoReferenceLookupDto>> Handle(GetPendingPoReferencesQuery request, CancellationToken cancellationToken)
            => await _queryRepository.GetRawMaterialPoReferencesAsync(request.Term);
    }
}
