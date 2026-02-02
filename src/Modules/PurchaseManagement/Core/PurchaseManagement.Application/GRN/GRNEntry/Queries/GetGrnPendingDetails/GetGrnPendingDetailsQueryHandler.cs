// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Inventory;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IParty;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails
// {
//     public class GetGrnPendingDetailsQueryHandler : IRequestHandler<GetGrnPendingDetailsQuery, List<GetGrnPendingDetailsDto>>
//     {
//         private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IInventoryGrpcClient _inventoryGrpcClient;
//         private readonly IPartyGrpcClient _partyGrpcClient;


//         public GetGrnPendingDetailsQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IInventoryGrpcClient inventoryGrpcClient, IPartyGrpcClient partyGrpcClient)
//         {
//             _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _inventoryGrpcClient = inventoryGrpcClient;
//             _partyGrpcClient = partyGrpcClient;

//         }

//         public async Task<List<GetGrnPendingDetailsDto>> Handle(    GetGrnPendingDetailsQuery request,    CancellationToken cancellationToken)
    
//         {
//             // 1) Fetch pending details
//             var result   = await _iGrnEntryQueryRepository.GetPendingGateEntriesForGrnAsync(request.GrnId ,request.IsGrnGenerated, request.IsQcGenerated);
//             var pending  = _mapper.Map<List<GetGrnPendingDetailsDto>>(result);
//               foreach (var po in pending)
//             {
//                 if (po.PartyId > 0)
//                 {
//                     var partyDetails = await _partyGrpcClient.GetPartyByIdAsync(po.PartyId);
//                     if (partyDetails != null)
//                     {
//                         po.PartyName = partyDetails.PartyName;
                                              
//                     }
//                 }
//             }

//             // 2) Gather unique ItemIds (no null-coalescing on different list types)
//             var itemIdSet = new HashSet<int>();
//             foreach (var header in pending)
//             {
//                 if (header?.GrnDetails == null) continue;
//                 foreach (var d in header.GrnDetails)
//                     if (d.ItemId > 0) itemIdSet.Add(d.ItemId);
//             }

//              // 3) Batch call to Inventory (one network hop)
//             Dictionary<int, InventoryQueryDto> tolMap = new();
//             if (itemIdSet.Count > 0)
//             {
//                 // If your IInventoryGrpcClient supports the batch overload:
//                 var tolList = await _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIdSet, cancellationToken);

          

//                 tolMap = tolList.GroupBy(t => t.ItemId).ToDictionary(g => g.Key, g => g.First());
//             }

//             // 4) Enrich all GRN details from the map
//             foreach (var header in pending)
//             {
//                 if (header?.GrnDetails == null) continue;

//                 foreach (var detail in header.GrnDetails)
//                 {
//                     if (!tolMap.TryGetValue(detail.ItemId, out var tol) || tol == null) continue;

//                     if (!string.IsNullOrWhiteSpace(tol.ItemName))
//                         detail.ItemName = tol.ItemName;

//                     if (!string.IsNullOrWhiteSpace(tol.UOMName))
//                         detail.UOMName = tol.UOMName;

//                     // If your detail tolerances are decimal?:

//                     detail.LowerTolerance = tol.LowerTolerance;
//                     detail.UpperTolerance = tol.UpperTolerance;
//                     detail.ItemCode = tol.ItemCode;
                
                   
//                 }
//             }

//             // 5) Domain event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "GetGrnPendingDetailsQuery",
//                 actionName: pending.Count.ToString(),
//                 details: "Pending PO details were fetched.",
//                 module: "GRNEntry");

//             await _mediator.Publish(domainEvent, cancellationToken);
//             return pending;

//         }
//     }
// }