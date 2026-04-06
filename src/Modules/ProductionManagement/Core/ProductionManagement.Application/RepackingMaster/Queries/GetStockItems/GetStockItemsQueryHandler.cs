using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetStockItems
{
    public class GetStockItemsQueryHandler : IRequestHandler<GetStockItemsQuery, List<StockItemSummaryDto>>
    {
        private readonly ISalesStockLedgerService _stockLedgerService;
        private readonly IIPAddressService _ipAddressService;

        public GetStockItemsQueryHandler(
            ISalesStockLedgerService stockLedgerService,
            IIPAddressService ipAddressService)
        {
            _stockLedgerService = stockLedgerService;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<StockItemSummaryDto>> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var result = await _stockLedgerService.GetStockItemsAsync(request.ProductionYear, unitId);
            return result.ToList();
        }
    }
}
