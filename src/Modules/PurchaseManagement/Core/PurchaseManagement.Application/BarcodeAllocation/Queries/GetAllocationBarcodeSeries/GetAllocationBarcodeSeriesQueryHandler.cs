using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationBarcodeSeries
{
    public class GetAllocationBarcodeSeriesQueryHandler : IRequestHandler<GetAllocationBarcodeSeriesQuery, IReadOnlyList<BarcodeAllocationSeriesDto>>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public GetAllocationBarcodeSeriesQueryHandler(IBarcodeAllocationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<BarcodeAllocationSeriesDto>> Handle(GetAllocationBarcodeSeriesQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetAvailableSeriesAsync(request.Term, request.SeriesId);
        }
    }
}
