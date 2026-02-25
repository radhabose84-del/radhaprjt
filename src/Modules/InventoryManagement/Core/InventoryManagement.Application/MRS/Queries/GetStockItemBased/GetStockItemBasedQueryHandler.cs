using AutoMapper;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetStockItemBased
{
    public class GetStockItemBasedQueryHandler : IRequestHandler<GetStockItemBasedQuery, List<GetStockItemDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        public GetStockItemBasedQueryHandler(IMapper mapper, IMediator mediator, IWarehouseLookup warehouseLookup, IMrsEntryQueryRepository iMrsEntryQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _warehouseLookup = warehouseLookup;
            _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
        }

        public async Task<List<GetStockItemDto>> Handle(GetStockItemBasedQuery request, CancellationToken cancellationToken)
        {
            // Fetch data from repository
            var result = await _iMrsEntryQueryRepository.GetStockDetails(request.ItemId,request.WarehouseId);

            // Map to DTOs (if needed — if repository already returns DTOs, you can skip this)
            var getStockItems = _mapper.Map<List<GetStockItemDto>>(result);

            var warehouseIds = getStockItems.Select(s => s.WarehouseId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var warehouseLookup = warehouseIds.Any()
                ? (await _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken))
                    .Where(w => w != null)
                    .ToDictionary(w => w.Id, w => w.WarehouseName)
                : new Dictionary<int, string>();

            foreach (var item in getStockItems)
            {
                if (warehouseLookup.TryGetValue(item.WarehouseId, out var warehouseName))
                    item.WarehouseName = warehouseName;
            }

            // Domain Event logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStockItemBasedQuery",
                actionName: getStockItems.Count.ToString(),
                details: $"Stock details was fetched.",
                module: "StockItem"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return getStockItems;
        }
    }
}
