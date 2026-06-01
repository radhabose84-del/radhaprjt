using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Gate;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending
{
    public class GetGrnPendingQueryHandler : IRequestHandler<GetGrnPendingQuery, List<GetGrnPendingDto>>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;
        private readonly IGateInwardLookup _gateInwardLookup;

        public GetGrnPendingQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IItemPurchaseToleranceLookup itemPurchaseToleranceLookup, IGateInwardLookup gateInwardLookup)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
            _gateInwardLookup = gateInwardLookup;
        }

         public async Task<List<GetGrnPendingDto>> Handle(GetGrnPendingQuery request, CancellationToken cancellationToken)
        {
            // 1) Fetch pending GRN data
            var result = await _iGrnEntryQueryRepository.GetPendingPoGrnAsync(
                request.PartyId, request.PoId, request.GateEntryId);

            var pending = _mapper.Map<List<GetGrnPendingDto>>(result);

            // 1.5) Cross-module: resolve GateEntryNo + GateEntryDate via the Gate lookup.
            // Replaces the dropped FROM Purchase.GateEntryHeader anchor. Cached.
            var gateEntryIds = pending
                .Where(x => x.GateEntryId > 0)
                .Select(x => x.GateEntryId)
                .Distinct()
                .ToList();

            if (gateEntryIds.Count > 0)
            {
                var gateInwardResult = await _gateInwardLookup.GetByIdsAsync(gateEntryIds, cancellationToken);
                var gateInwardMap = gateInwardResult.ToDictionary(g => g.Id, g => g);
                foreach (var header in pending)
                {
                    if (header.GateEntryId > 0 && gateInwardMap.TryGetValue(header.GateEntryId, out var gate))
                    {
                        header.GateEntryNo = gate.GateEntryNo;
                        header.GateEntryDate = gate.GateEntryDate;
                    }
                }
            }

            // 2) Gather unique ItemIds across all details
            var itemIds = pending
                        .SelectMany(h => h.GrnDetails ?? Enumerable.Empty<GetGrnPendingDto.GetGrnPendingDetailsGateDto>())
                        .Select(d => d.ItemId)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToArray();

            if (itemIds.Length > 0)
            {
                // 3) Batch call to Inventory for tolerances
               // var tolerances = await _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);
                 var tolerances = await _itemPurchaseToleranceLookup.GetByIdsAsync(itemIds.ToList(), cancellationToken);

                // Build a quick lookup
                var tolMap = tolerances.ToDictionary(t => t.ItemId, t => t);

                // 4) Enrich GRN details
                foreach (var header in pending)
                {
                    if (header.GrnDetails == null) continue;

                    foreach (var detail in header.GrnDetails)
                    {
                        if (!tolMap.TryGetValue(detail.ItemId, out var tol) || tol is null) continue;

                        // Names/UOM: prefer service values when present
                        if (!string.IsNullOrWhiteSpace(tol.ItemName))
                            detail.ItemName = tol.ItemName;

                        if (!string.IsNullOrWhiteSpace(tol.UOMName))
                            detail.UOMName = tol.UOMName;

                        // Tolerances:
                        // If your DTO props are decimal? just assign; if decimal, coalesce as needed
                        detail.LowerTolerance = tol.LowerTolerance;              // decimal?
                        detail.UpperTolerance = tol.UpperTolerance;              // decimal?
                        detail.ItemCode = tol.ItemCode;
                    
                    }
                }
            }

            // 5) Domain event (unchanged)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGrnPendingQuery",
                actionName: pending.Count.ToString(),
                details: "Pending PO details were fetched.",
                module: "GRNEntry");

            await _mediator.Publish(domainEvent, cancellationToken);

            return pending;
           
        }
    }
}