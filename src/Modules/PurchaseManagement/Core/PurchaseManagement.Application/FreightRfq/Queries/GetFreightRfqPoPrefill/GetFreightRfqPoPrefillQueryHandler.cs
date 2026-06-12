using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPoPrefill
{
    public class GetFreightRfqPoPrefillQueryHandler : IRequestHandler<GetFreightRfqPoPrefillQuery, FreightRfqPoPrefillDto?>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;

        public GetFreightRfqPoPrefillQueryHandler(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<FreightRfqPoPrefillDto?> Handle(GetFreightRfqPoPrefillQuery request, CancellationToken cancellationToken)
            => await _queryRepository.GetPoPrefillAsync(request.PoId);
    }
}
