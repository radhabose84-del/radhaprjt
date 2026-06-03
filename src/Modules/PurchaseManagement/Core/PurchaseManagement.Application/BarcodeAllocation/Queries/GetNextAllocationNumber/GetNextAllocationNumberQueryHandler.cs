using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationNumber
{
    public class GetNextAllocationNumberQueryHandler : IRequestHandler<GetNextAllocationNumberQuery, string>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public GetNextAllocationNumberQueryHandler(IBarcodeAllocationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<string> Handle(GetNextAllocationNumberQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.PeekNextAllocationNumberAsync(request.AllocationDate);
        }
    }
}
