using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeSeriesNumber
{
    public class GetNextBarcodeSeriesNumberQueryHandler : IRequestHandler<GetNextBarcodeSeriesNumberQuery, string>
    {
        private readonly IBarcodeSeriesQueryRepository _queryRepository;

        public GetNextBarcodeSeriesNumberQueryHandler(IBarcodeSeriesQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<string> Handle(GetNextBarcodeSeriesNumberQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.PeekNextSeriesNumberAsync(request.GenerationDate);
        }
    }
}
