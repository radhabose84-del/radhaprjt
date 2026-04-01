using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetStockItems
{
    public class GetStockItemsQueryHandler
        : IRequestHandler<GetStockItemsQuery, IReadOnlyList<StockItemSummaryDto>>
    {
        private readonly ISalesStockLedgerService _stockLedgerLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public GetStockItemsQueryHandler(
            ISalesStockLedgerService stockLedgerLookup,
            IItemLookup itemLookup,
            IPackTypeLookup packTypeLookup,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _stockLedgerLookup = stockLedgerLookup;
            _itemLookup = itemLookup;
            _packTypeLookup = packTypeLookup;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<IReadOnlyList<StockItemSummaryDto>> Handle(
            GetStockItemsQuery request,
            CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var items = await _stockLedgerLookup.GetStockItemsAsync(request.ProductionYear, unitId, request.PackTypeId, cancellationToken);

            // Populate names from cross-module lookups
            var itemIds = items.Select(i => i.ItemId).Distinct();
            var allItems = await _itemLookup.GetByIdsAsync(itemIds, cancellationToken);
            var itemDict = allItems.ToDictionary(i => i.Id, i => i.ItemName);

            var packTypeIds = items.Select(i => i.PackTypeId).Distinct();
            var allPackTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds, cancellationToken);
            var packTypeDict = allPackTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

            foreach (var item in items)
            {
                item.ItemName = itemDict.TryGetValue(item.ItemId, out var name) ? name : null;
                item.PackTypeName = packTypeDict.TryGetValue(item.PackTypeId, out var ptName) ? ptName : null;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStockItems",
                actionCode: "GetStockItemsQuery",
                actionName: items.Count.ToString(),
                details: "Stock items summary was fetched.",
                module: "RepackingMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return items;
        }
    }
}
