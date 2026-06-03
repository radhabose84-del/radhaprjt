using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationFrom
{
    public class GetNextAllocationFromQueryHandler : IRequestHandler<GetNextAllocationFromQuery, long>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public GetNextAllocationFromQueryHandler(IBarcodeAllocationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<long> Handle(GetNextAllocationFromQuery request, CancellationToken cancellationToken)
        {
            var maxAllocatedTo = await _queryRepository.GetMaxAllocatedToForSeriesAsync(request.BarcodeSeriesId);
            return (maxAllocatedTo ?? 0) + 1;
        }
    }
}
