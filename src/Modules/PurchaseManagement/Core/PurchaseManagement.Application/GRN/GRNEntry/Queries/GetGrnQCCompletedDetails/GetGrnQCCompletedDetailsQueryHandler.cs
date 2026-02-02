// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
// {
//     public class GetGrnQCCompletedDetailsQueryHandler : IRequestHandler<GetGrnQCCompletedDetailsQuery, List<GetGrnQCCompletedDto>>
//     {

//         private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IInventoryGrpcClient _inventoryGrpcClient;
//         private readonly IPutawayRuleGrpcClient _putawayRuleGrpcClient;

//         public GetGrnQCCompletedDetailsQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IInventoryGrpcClient inventoryGrpcClient, IPutawayRuleGrpcClient putawayRuleGrpcClient)
//         {
//             _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _inventoryGrpcClient = inventoryGrpcClient;
//             _putawayRuleGrpcClient = putawayRuleGrpcClient;
//         }

//        public async Task<List<GetGrnQCCompletedDto>> Handle(GetGrnQCCompletedDetailsQuery request, CancellationToken cancellationToken)
//         {
//             // 1️⃣ Fetch GRN details from repository
//             var result = await _iGrnEntryQueryRepository.GetGrnQcCompletedDetails(request.GrnId ??0,request.ItemId ??0);
//             var pendingpoIds = _mapper.Map<List<GetGrnQCCompletedDto>>(result);
//           // ⭐ QcWarehouseId should come from DTO header
//             var warehouseId = pendingpoIds.FirstOrDefault()?.QcWarehouseId ?? 0;

//             var warehouseIds = new List<int> { warehouseId }; 

//             // 2️⃣ Gather unique ItemIds across all GRN details
//             var itemIds = pendingpoIds
//                 .SelectMany(h => h.GrnDetails ?? Enumerable.Empty<GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails>())
//                 .Select(d => d.ItemId)
//                 .Where(id => id > 0)
//                 .Distinct()
//                 .ToArray();

//             if (itemIds.Length > 0)
//             {
//                 // 3️⃣ Fetch tolerance data from Inventory gRPC
//                 var tolerances = await _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds.ToList(),cancellationToken);
//                 var tolMap = tolerances.ToDictionary(t => t.ItemId, t => t);

//                 // 4️⃣ Fetch Putaway rule data from PutawayRule gRPC
//                 var putawayRules = await _putawayRuleGrpcClient.GetPutAwayRuleDetailsAsync(itemIds.ToList(), warehouseIds.ToList() ,cancellationToken);

//                 // Group all rules by ItemId to keep all rules, not just first
//                 var putawayMap = putawayRules
//                     .GroupBy(p => p.ItemId)
//                     .ToDictionary(g => g.Key, g => g.ToList());

//                 // 5️⃣ Enrich each GRN detail with tolerance & putaway rule data
//                 foreach (var header in pendingpoIds)
//                 {
//                     if (header.GrnDetails == null) continue;

//                     foreach (var detail in header.GrnDetails)
//                     {
//                         // a) Add tolerance data
//                         if (tolMap.TryGetValue(detail.ItemId, out var tol) && tol != null)
//                         {
//                             if (!string.IsNullOrWhiteSpace(tol.ItemName))
//                                 detail.ItemName = tol.ItemName;

//                             if (!string.IsNullOrWhiteSpace(tol.UOMName))
//                                 detail.UOMName = tol.UOMName;

//                             detail.ItemCode = tol.ItemCode;
//                         }

//                         // b) Add all Putaway rules for this item
//                         if (putawayMap.TryGetValue(detail.ItemId, out var rules) && rules != null)
// {
//                             detail.PutawayRules = rules.Select(rule =>
//                             {
//                                 var conversionRate = rule.ConversionRate ?? 0;
//                                 var qcAcceptedQty = detail.QcAcceptedQuantity ?? 0;

//                                 // ✅ Apply business logic:
//                                 // If conversion rate is 0 or null -> use same qty
//                                 // Else -> multiply by conversion rate
//                                 var calculatedQty = conversionRate > 0 
//                                     ? qcAcceptedQty * (decimal)conversionRate 
//                                     : qcAcceptedQty;

//                                 return new GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails.PutawayRuleDto
//                                 {
//                                     PutAwayRuleId = rule.PutAwayRuleId,
//                                     StorageTypeId = rule.StorageTypeId,
//                                     StorageTypeName = rule.StorageTypeName,
//                                     TargetId = rule.TargetId,
//                                     TargetCode = rule.TargetCode,
//                                     TargetName = rule.TargetName,
//                                     PriorityId = rule.PriorityId,
//                                     PriorityName = rule.PriorityName,
//                                     WarehouseId = rule.WarehouseId,
//                                     WarehouseCode = rule.WarehouseCode,
//                                     WarehouseName = rule.WarehouseName,
//                                     ItemId = rule.ItemId,
//                                     ItemCode = rule.ItemCode,
//                                     ItemCategoryName = rule.ItemCategoryName,
//                                     ItemGroupName = rule.ItemGroupName,
//                                     StockUomId = rule.StockUomId,
//                                     StockUom = rule.StockUom,
//                                     PurchaseUomId = rule.PurchaseUomId,
//                                     PurchaseUom = rule.PurchaseUom,
//                                     ItemName = rule.ItemName,
//                                     ConversionRate = rule.ConversionRate,
//                                     // ✅ Add calculated quantity for display
//                                     CalculatedPutawayQty = calculatedQty
//                                 };
//                             }).ToList();
//                         }
//                     }
//                 }
//             }

//             // 6️⃣ Publish domain event for audit logs
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "GetGrnQCCompletedDetailsQuery",
//                 actionName: pendingpoIds.Count.ToString(),
//                 details: $"Pending PO details were fetched.",
//                 module: "GRNEntry"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             return pendingpoIds;
//         }
//     }
// }