using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotByStock
{
    public class GetLotByStockQueryHandler : IRequestHandler<GetLotByStockQuery, IReadOnlyList<StockLotByItemDto>>
    {
        private readonly ISalesStockLedgerService _stockLedgerService;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetLotByStockQueryHandler(
            ISalesStockLedgerService stockLedgerService,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _stockLedgerService = stockLedgerService;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<StockLotByItemDto>> Handle(GetLotByStockQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var result = await _stockLedgerService.GetLotByStockAsync(
                request.ItemId, request.SourceUnitId, unitId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLotByStockQuery",
                actionCode: "Get",
                actionName: $"ItemId:{request.ItemId}",
                details: "Lot by stock details were fetched.",
                module: "LotMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
