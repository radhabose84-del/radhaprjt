// using AutoMapper;
// using Contracts.Dtos.Inventory;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using PurchaseManagement.Application.Common.Exceptions;
// using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
// using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
// using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries;

// public class GetRfqByIdQueryHandler : IRequestHandler<GetRfqByIdQuery, RfqDto>
// {
//     private readonly IRfqQueryRepository _repo;
//     private readonly IMapper _mapper;
//     private readonly IMediator _mediator;
//     private readonly IItemGrpcClient _itemGrpc;
//     private readonly IUOMGrpcClient _uomGrpc;
//     private readonly IHSNGrpcClient _hsnGrpc;

//     public GetRfqByIdQueryHandler(
//         IRfqQueryRepository repo,
//         IMapper mapper,
//         IMediator mediator,
//         IItemGrpcClient itemGrpc,
//         IUOMGrpcClient uomGrpc,
//         IHSNGrpcClient hsnGrpc)
//     {
//         _repo = repo;
//         _mapper = mapper;
//         _mediator = mediator;
//         _itemGrpc = itemGrpc;
//         _uomGrpc = uomGrpc;
//         _hsnGrpc = hsnGrpc;
//     }

//     public async Task<RfqDto> Handle(GetRfqByIdQuery request, CancellationToken ct)
//     {
//         var agg = await _repo.GetAggregateAsync(request.Id, ct)
//                   ?? throw new ExceptionRules("RFQ not found.");

//         var result = _mapper.Map<RfqDto>(agg);

//         if (result.Items == null || result.Items.Length == 0)
//         {
//             await PublishAudit(result, ct);
//             return result;
//         }

//         // Distinct IDs for lookups
//         var itemIds = result.Items.Select(i => i.ItemId).Where(id => id > 0).Distinct().ToList();
//         var uomIds  = result.Items.Select(i => i.UomId).Where(id => id > 0).Distinct().ToList();
//         var hsnIds  = result.Items.Select(i => i.HsnId).Where(id => id > 0).Distinct().ToList();

//         // Parallel lookups
//         var itemsTask = _itemGrpc.GetItemsByIdsAsync(itemIds, ct);
//         var uomTasks  = uomIds.Select(id => _uomGrpc.GetByIdAsync(id, ct)).ToArray();
         
//         Task<Contracts.Dtos.Inventory.HSNMasterDto>[] hsnTasks = Array.Empty<Task<HSNMasterDto>>();
//             if (hsnIds.Count > 0)
//                 hsnTasks = hsnIds.Select(id => _hsnGrpc.GetByIdAsync(id)).ToArray();

//         // Await all
//         await Task.WhenAll(
//             itemsTask,
//             Task.WhenAll(uomTasks),
//             Task.WhenAll(hsnTasks)
//         );

//         // Build maps
//         var itemsFromGrpc = await itemsTask; // List<ItemMasterDto>
//         var itemDetailMap = itemsFromGrpc
//             .GroupBy(x => x.Id)
//             .ToDictionary(g => g.Key, g => g.First());

//         var itemNameMap = itemsFromGrpc
//             .GroupBy(x => x.Id)
//             .ToDictionary(
//                 g => g.Key,
//                 g =>
//                 {
//                     var it = g.First();
//                     return string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemCode : it.ItemName;
//                 });

//         var uomMap = uomTasks
//             .Where(t => t.Result != null)
//             .Select(t => t.Result!)
//             .ToDictionary(
//                 u => u.Id,
//                 u => string.IsNullOrWhiteSpace(u.UOMName)
//                         ? (u.Code ?? u.Id.ToString())
//                         : u.UOMName);

//         var hsnMap = hsnTasks
//             .Where(t => t.Result != null)
//             .Select(t => t.Result!)
//             .GroupBy(h => h.Id)
//             .ToDictionary(g => g.Key, g => g.First()); // HSN Id -> HSN dto (has GSTPercentage)

//         // Rebuild items with names, GST & ItemCategoryId from gRPC (when missing in RFQ)
//         var enrichedItems = result.Items.Select(i =>
//         {
//             itemNameMap.TryGetValue(i.ItemId, out var itemName);
//             uomMap.TryGetValue(i.UomId, out var uomName);
//             itemDetailMap.TryGetValue(i.ItemId, out var grpcItem);

//             // GST priority: HSN (if available) -> RFQ -> gRPC item
//             decimal gst = i.GstPercentage;
//             if (i.HsnId > 0 && hsnMap.TryGetValue(i.HsnId, out var hsnDto))
//                 gst = hsnDto.GSTPercentage;
//             else if (gst <= 0 && grpcItem is not null)
//                 gst = grpcItem.GSTPercentage;

//             // ItemCategoryId: RFQ if present, else gRPC item
//             int itemCategoryId = i.ItemCategoryId > 0
//                 ? i.ItemCategoryId
//                 : (grpcItem?.ItemCategoryId ?? 0);

//             return new RfqItemDto(
//                 i.ItemId,
//                 i.Qty,
//                 i.UomId,
//                 uomName ?? string.Empty,
//                 itemName ?? string.Empty,
//                 gst,
//                 i.HsnId,
//                 itemCategoryId
//             );
//         }).ToArray();

//         var enriched = result with { Items = enrichedItems };

//         await PublishAudit(enriched, ct);
//         return enriched;
//     }

//     private async Task PublishAudit(RfqDto dto, CancellationToken ct)
//     {
//         var domainEvent = new AuditLogsDomainEvent(
//             actionDetail: "GetById",
//             actionCode: string.Empty,
//             actionName: string.Empty,
//             details: $"Rfq details {dto.Id} was fetched.",
//             module: "RFQ");
//         await _mediator.Publish(domainEvent, ct);
//     }
// }
