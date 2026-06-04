using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeStartNumber
{
    public class GetNextBarcodeStartNumberQueryHandler : IRequestHandler<GetNextBarcodeStartNumberQuery, long>
    {
        private readonly IBarcodeSeriesQueryRepository _queryRepository;

        public GetNextBarcodeStartNumberQueryHandler(IBarcodeSeriesQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<long> Handle(GetNextBarcodeStartNumberQuery request, CancellationToken cancellationToken)
        {
            var maxEnd = await _queryRepository.GetMaxEndNumberAsync();
            return (maxEnd ?? 0) + 1;
        }
    }
}
