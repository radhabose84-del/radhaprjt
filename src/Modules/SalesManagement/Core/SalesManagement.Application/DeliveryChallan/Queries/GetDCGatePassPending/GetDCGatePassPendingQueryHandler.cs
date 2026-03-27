using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending
{
    public sealed class GetDCGatePassPendingQueryHandler
        : IRequestHandler<GetDCGatePassPendingQuery, List<GetDCGatePassPendingDto>>
    {
        private readonly IDeliveryChallanQueryRepository _repo;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly ILotMasterLookup _lotMasterLookup;
        private readonly IMediator _mediator;

        public GetDCGatePassPendingQueryHandler(
            IDeliveryChallanQueryRepository repo,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            ILotMasterLookup lotMasterLookup,
            IMediator mediator)
        {
            _repo = repo;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _lotMasterLookup = lotMasterLookup;
            _mediator = mediator;
        }

        public async Task<List<GetDCGatePassPendingDto>> Handle(
            GetDCGatePassPendingQuery request, CancellationToken ct)
        {
            var pending = await _repo.GetDCGatePassPendingAsync(request.VehicleNo);

            if (pending.Count == 0)
            {
                await PublishAudit(0, ct);
                return pending;
            }

            // Collect IDs for cross-module enrichment
            var allPlantIds = pending.Select(r => r.FromPlantId)
                .Union(pending.Select(r => r.ToPlantId))
                .Distinct().ToList();

            var warehouseIds = pending.Select(r => r.FromStorageLocationId)
                .Union(pending.Select(r => r.ToStorageLocationId))
                .Distinct().ToList();

            var transporterIds = pending.Select(r => r.TransporterId).Distinct().ToList();

            var allDetails = pending
                .SelectMany(r => r.DeliveryChallanDetails ?? new List<GetDCGatePassPendingDto.GetDCGatePassPendingDetailDto>())
                .ToList();

            var itemIds = allDetails
                .Select(d => d.ItemId)
                .Where(id => id > 0)
                .Distinct().ToList();

            var uomIds = allDetails
                .Select(d => d.UOMId)
                .Where(id => id > 0)
                .Distinct().ToList();

            var lotIds = allDetails
                .Where(d => d.LotId > 0)
                .Select(d => d.LotId)
                .Distinct().ToList();

            // Parallel cross-module lookups
            var plantTask = _unitLookup.GetByIdsAsync(allPlantIds);
            var warehouseTask = _warehouseLookup.GetByIdsAsync(warehouseIds);
            var transporterTask = _partyLookup.GetByIdsAsync(transporterIds, ct);
            var itemTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
            var uomTask = uomIds.Count > 0
                ? _uomLookup.GetByIdsAsync(uomIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());
            var lotTask = lotIds.Count > 0
                ? _lotMasterLookup.GetByIdsAsync(lotIds)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Production.LotMasterLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Production.LotMasterLookupDto>());

            await Task.WhenAll(plantTask, warehouseTask, transporterTask, itemTask, uomTask, lotTask);

            // Build lookup dictionaries
            var plantDict = (await plantTask).ToDictionary(p => p.UnitId, p => p.UnitName);
            var warehouseDict = (await warehouseTask).ToDictionary(w => w.Id, w => w.WarehouseName);
            var transporterDict = (await transporterTask).ToDictionary(t => t.Id, t => t.PartyName);

            var itemDict = (await itemTask)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var it = g.First();
                    return (Code: it.ItemCode ?? string.Empty,
                            Name: !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName : (it.ItemCode ?? string.Empty));
                });

            var uomDict = (await uomTask)
                .GroupBy(u => u.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var u = g.First();
                    return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : (u.Code ?? u.Id.ToString());
                });

            var lotDict = (await lotTask).ToDictionary(l => l.Id, l => l.LotCode);

            // Enrich
            foreach (var r in pending)
            {
                r.FromPlantName = plantDict.TryGetValue(r.FromPlantId, out var fpName) ? fpName : null;
                r.ToPlantName = plantDict.TryGetValue(r.ToPlantId, out var tpName) ? tpName : null;
                r.FromStorageLocationName = warehouseDict.TryGetValue(r.FromStorageLocationId, out var fsName) ? fsName : null;
                r.ToStorageLocationName = warehouseDict.TryGetValue(r.ToStorageLocationId, out var tsName) ? tsName : null;
                r.TransporterName = transporterDict.TryGetValue(r.TransporterId, out var trName) ? trName : null;

                if (r.DeliveryChallanDetails == null) continue;

                foreach (var detail in r.DeliveryChallanDetails)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.Code;
                        detail.ItemName = itemInfo.Name;
                    }

                    if (uomDict.TryGetValue(detail.UOMId, out var uName))
                        detail.UOMName = uName;

                    if (detail.LotId > 0 && lotDict.TryGetValue(detail.LotId, out var lotCode))
                        detail.LotCode = lotCode;
                }
            }

            await PublishAudit(pending.Count, ct);
            return pending;
        }

        private Task PublishAudit(int count, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-DCGatePassPending",
                    actionCode: string.Empty,
                    actionName: "DCGatePassPending",
                    details: $"Fetched {count} delivery challans pending for gate pass.",
                    module: "DeliveryChallan"), ct);
    }
}
