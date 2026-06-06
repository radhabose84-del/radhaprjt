using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetNextFreightRfqNumber
{
    public class GetNextFreightRfqNumberQueryHandler : IRequestHandler<GetNextFreightRfqNumberQuery, string>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;

        public GetNextFreightRfqNumberQueryHandler(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<string> Handle(GetNextFreightRfqNumberQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.PeekNextNumberAsync(request.RfqDate);
        }
    }
}
