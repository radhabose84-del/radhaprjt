// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IWarehouse;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader
// {
//     public class GetGrnPendingHeaderQueryHandler : IRequestHandler<GetGrnPendingHeaderQuery, ApiResponseDTO<List<GetGrnPendingHeaderDto>>>
//     {
//         private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IPartyGrpcClient _partyGrpcClient;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;

//         public GetGrnPendingHeaderQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IPartyGrpcClient partyGrpcClient, IWarehouseGrpcClient warehouseGrpcClient)
//         {
//             _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _partyGrpcClient = partyGrpcClient;
//             _warehouseGrpcClient = warehouseGrpcClient;
//         }

        

//         public async  Task<ApiResponseDTO<List<GetGrnPendingHeaderDto>>> Handle(GetGrnPendingHeaderQuery request, CancellationToken cancellationToken)
//         {
//            var (result, totalCount) = await _iGrnEntryQueryRepository.GetPendingGrnHeaderAsync(
//             request.FromDate,
//             request.ToDate,
//             request.IsGrnGenerated,
//             request.IsQcGenerated,
//             request.PageNumber,
//             request.PageSize,
//             request.SearchTerm
//         );

//         var pendingGrnList = _mapper.Map<List<GetGrnPendingHeaderDto>>(result);

//         // Collect unique Party and Warehouse IDs
//         var partyIds = pendingGrnList
//             .Where(x => x.PartyId > 0)
//             .Select(x => x.PartyId)
//             .Distinct()
//             .ToList();

//         var warehouseIds = pendingGrnList
//             .Where(x => (x.ReceivingWarehouseId.HasValue && x.ReceivingWarehouseId > 0)
//                     || (x.QcWarehouseId.HasValue && x.QcWarehouseId > 0))
//             .Select(x => x.ReceivingWarehouseId ?? x.QcWarehouseId!.Value)
//             .Distinct()
//             .ToList();

//         // Fire gRPC calls concurrently
//         var partyTasks = partyIds.Select(id => _partyGrpcClient.GetPartyByIdAsync(id)).ToList();
//         var warehouseTasks = warehouseIds.Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();

//         // Await all calls at once
//          // Wait for all calls to complete
//         await Task.WhenAll(Task.WhenAll(partyTasks), Task.WhenAll(warehouseTasks));
// ;

//         // Map results into dictionaries
//         var partyResults = partyTasks
//             .Where(t => t.Result != null)
//             .Select(t => t.Result)
//             .ToDictionary(p => p.Id, p => p);

//         var warehouseResults = warehouseTasks
//             .Where(t => t.Result != null)
//             .Select(t => t.Result)
//             .ToDictionary(w => w.Id, w => w);

//         // Enrich DTOs
//         foreach (var po in pendingGrnList)
//         {
//             // 🧾 Party name
//             if (po.PartyId > 0 && partyResults.TryGetValue(po.PartyId, out var party))
//                 po.PartyName = party.PartyName;
//             else
//                 po.PartyName = "NA";

//             // 🏭 Receiving Warehouse
//             if (po.ReceivingWarehouseId.HasValue &&
//                 warehouseResults.TryGetValue(po.ReceivingWarehouseId.Value, out var receivingWarehouse))
//                 po.ReceivingWarehouseName = receivingWarehouse.WarehouseName;
//             else
//                 po.ReceivingWarehouseName = "NA";

//             // 🧪 QC Warehouse
//             if (po.QcWarehouseId.HasValue &&
//                 warehouseResults.TryGetValue(po.QcWarehouseId.Value, out var qcWarehouse))
//                 po.QcWarehouseName = qcWarehouse.WarehouseName;
//             else
//                 po.QcWarehouseName = "NA";
//         }
//              //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetAll",
//                     actionCode: "GetGrnPendingHeaderQueryHandler",        
//                     actionName: pendingGrnList.Count.ToString(),
//                     details: $"Pending PO details was fetched.",
//                     module:"GRNEntry"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//               return new ApiResponseDTO<List<GetGrnPendingHeaderDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = pendingGrnList,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }
//     }
// }