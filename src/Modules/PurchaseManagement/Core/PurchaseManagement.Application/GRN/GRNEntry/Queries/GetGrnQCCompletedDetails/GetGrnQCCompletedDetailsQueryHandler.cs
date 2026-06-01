using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Gate;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
{
    public class GetGrnQCCompletedDetailsQueryHandler : IRequestHandler<GetGrnQCCompletedDetailsQuery, List<GetGrnQCCompletedDto>>
    {

        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;
        private readonly IPutawayRuleLookup _putawayRuleLookup;
        private readonly IGateInwardLookup _gateInwardLookup;

        public GetGrnQCCompletedDetailsQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IItemPurchaseToleranceLookup itemPurchaseToleranceLookup, IPutawayRuleLookup putawayRuleLookup, IGateInwardLookup gateInwardLookup)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
            _putawayRuleLookup = putawayRuleLookup;
            _gateInwardLookup = gateInwardLookup;
        }

       public async Task<List<GetGrnQCCompletedDto>> Handle(GetGrnQCCompletedDetailsQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Fetch GRN details from repository
            var result = await _iGrnEntryQueryRepository.GetGrnQcCompletedDetails(request.GrnId ??0,request.ItemId ??0);
            var pendingpoIds = _mapper.Map<List<GetGrnQCCompletedDto>>(result);

            // 1.5️⃣ Cross-module: resolve GateEntryNo + GateEntryDate from Gate.GateInwardHdr
            // Replaces the dropped INNER JOIN to Purchase.GateEntryHeader. Cached lookup.
            var gateEntryIds = pendingpoIds
                .Where(x => x.GateEntryId > 0)
                .Select(x => x.GateEntryId)
                .Distinct()
                .ToList();

            if (gateEntryIds.Count > 0)
            {
                var gateInwardResult = await _gateInwardLookup.GetByIdsAsync(gateEntryIds, cancellationToken);
                var gateInwardMap = gateInwardResult.ToDictionary(g => g.Id, g => g);
                foreach (var header in pendingpoIds)
                {
                    if (header.GateEntryId > 0 && gateInwardMap.TryGetValue(header.GateEntryId, out var gate))
                    {
                        header.GateEntryNo = gate.GateEntryNo;
                        header.GateEntryDate = gate.GateEntryDate;
                    }
                }
            }

          // ⭐ QcWarehouseId should come from DTO header
            var warehouseId = pendingpoIds.FirstOrDefault()?.QcWarehouseId ?? 0;

            var warehouseIds = new List<int> { warehouseId }; 

            // 2️⃣ Gather unique ItemIds across all GRN details
            var itemIds = pendingpoIds
                .SelectMany(h => h.GrnDetails ?? Enumerable.Empty<GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails>())
                .Select(d => d.ItemId)
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (itemIds.Length > 0)
            {
                // 3️⃣ Fetch tolerance data from Inventory gRPC
                var tolerances = await _itemPurchaseToleranceLookup.GetByIdsAsync(itemIds.ToList(),cancellationToken);
                var tolMap = tolerances.ToDictionary(t => t.ItemId, t => t);

                // 4️⃣ Fetch Putaway rule data from PutawayRule gRPC
                var putawayRules = await _putawayRuleLookup.GetByIdsAsync(itemIds.ToList(), warehouseIds.ToList() ,cancellationToken);

                // Group all rules by ItemId to keep all rules, not just first
                var putawayMap = putawayRules
                    .GroupBy(p => p.ItemId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 5️⃣ Enrich each GRN detail with tolerance & putaway rule data
                foreach (var header in pendingpoIds)
                {
                    if (header.GrnDetails == null) continue;

                    foreach (var detail in header.GrnDetails)
                    {
                        // a) Add tolerance data
                        if (tolMap.TryGetValue(detail.ItemId, out var tol) && tol != null)
                        {
                            if (!string.IsNullOrWhiteSpace(tol.ItemName))
                                detail.ItemName = tol.ItemName;

                            if (!string.IsNullOrWhiteSpace(tol.UOMName))
                                detail.UOMName = tol.UOMName;

                            detail.ItemCode = tol.ItemCode;
                        }

                        // b) Add all Putaway rules for this item
                        if (putawayMap.TryGetValue(detail.ItemId, out var rules) && rules != null)
{
                            detail.PutawayRules = rules.Select(rule =>
                            {
                                var conversionRate = rule.ConversionRate ?? 0;
                                var qcAcceptedQty = detail.QcAcceptedQuantity ?? 0;

                                // ✅ Apply business logic:
                                // If conversion rate is 0 or null -> use same qty
                                // Else -> multiply by conversion rate
                                var calculatedQty = conversionRate > 0 
                                    ? qcAcceptedQty * (decimal)conversionRate 
                                    : qcAcceptedQty;

                                return new GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails.PutawayRuleDto
                                {
                                    PutAwayRuleId = rule.PutAwayRuleId,
                                    StorageTypeId = rule.StorageTypeId,
                                    StorageTypeName = rule.StorageTypeName,
                                    TargetId = rule.TargetId,
                                    TargetCode = rule.TargetCode,
                                    TargetName = rule.TargetName,
                                    PriorityId = rule.PriorityId,
                                    PriorityName = rule.PriorityName,
                                    WarehouseId = rule.WarehouseId,
                                    WarehouseCode = rule.WarehouseCode,
                                    WarehouseName = rule.WarehouseName,
                                    ItemId = rule.ItemId,
                                    ItemCode = rule.ItemCode,
                                    ItemCategoryName = rule.ItemCategoryName,
                                    ItemGroupName = rule.ItemGroupName,
                                    StockUomId = rule.StockUomId,
                                    StockUom = rule.StockUom,
                                    PurchaseUomId = rule.PurchaseUomId,
                                    PurchaseUom = rule.PurchaseUom,
                                    ItemName = rule.ItemName,
                                    ConversionRate = rule.ConversionRate,
                                    // ✅ Add calculated quantity for display
                                    CalculatedPutawayQty = calculatedQty
                                };
                            }).ToList();
                        }
                    }
                }
            }

            // 6️⃣ Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGrnQCCompletedDetailsQuery",
                actionName: pendingpoIds.Count.ToString(),
                details: $"Pending PO details were fetched.",
                module: "GRNEntry"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return pendingpoIds;
        }
    }
}