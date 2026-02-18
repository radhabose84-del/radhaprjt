// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IWarehouse;
// using Contracts.Common;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Domain.Events;
// using MassTransit.Futures.Contracts;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
// {
//     public class GetGrnQCCompletedDetailsHeaderQueryHandler : IRequestHandler<GetGrnQCCompletedDetailsHeaderQuery,  ApiResponseDTO<List<GetGrnQCCompletedDetailsDto>>>
//     {
//         private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IPartyGrpcClient _partyGrpcClient;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;

//         public GetGrnQCCompletedDetailsHeaderQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IPartyGrpcClient partyGrpcClient, IWarehouseGrpcClient warehouseGrpcClient)
//         {
//             _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _partyGrpcClient = partyGrpcClient;
//             _warehouseGrpcClient = warehouseGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<GetGrnQCCompletedDetailsDto>>> Handle(GetGrnQCCompletedDetailsHeaderQuery request, CancellationToken cancellationToken)
//         {
//             var (result, totalCount) = await _iGrnEntryQueryRepository.GetGrnQcCompletedHeader(
//             request.FromDate,
//             request.ToDate,
//             request.PageNumber,
//             request.PageSize,
//             request.SearchTerm
//         );

//         var pendingpoIds = _mapper.Map<List<GetGrnQCCompletedDetailsDto>>(result);

//         // ✅ 1️⃣ Collect distinct Party IDs (for batch gRPC)
//         var partyIds = pendingpoIds
//             .Where(p => p.PartyId > 0)
//             .Select(p => p.PartyId)
//             .Distinct()
//             .ToList();

//         // ✅ 2️⃣ Collect all distinct warehouse IDs (Receiving + QC)
//         var allWarehouseIds = pendingpoIds
//             .SelectMany(x => new[] { x.ReceivingWarehouseId, x.QcWarehouseId })
//             .Where(id => id.HasValue)
//             .Select(id => id.Value)
//             .Distinct()
//             .ToList();

//         // ✅ 3️⃣ Fire all gRPC calls concurrently
//         var partyTasks = partyIds.Select(id => _partyGrpcClient.GetPartyByIdAsync(id)).ToList();
//         var warehouseTasks = allWarehouseIds.Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();

//         // Wait for all calls to complete
//         await Task.WhenAll(Task.WhenAll(partyTasks), Task.WhenAll(warehouseTasks));

//         // ✅ 4️⃣ Build lookup dictionaries
//         var partyDtos = partyTasks
//             .Select(t => t.Result)
//             .Where(x => x != null)
//             .ToDictionary(x => x.Id, x => x.PartyName);

//         var warehouseDtos = warehouseTasks
//             .Select(t => t.Result)
//             .Where(x => x != null)
//             .ToDictionary(x => x.Id, x => x.WarehouseName);

//         // ✅ 5️⃣ Enrich the results with names
//         foreach (var po in pendingpoIds)
//         {
//             // 🧾 Party name
//             if (po.PartyId > 0 && partyDtos.TryGetValue(po.PartyId, out var partyName))
//                 po.PartyName = partyName;
//             else
//                 po.PartyName = "NA";

//             // 🏭 Receiving Warehouse
//             if (po.ReceivingWarehouseId.HasValue &&
//                 warehouseDtos.TryGetValue(po.ReceivingWarehouseId.Value, out var receivingName))
//                 po.ReceivingWarehouseName = receivingName;
//             else
//                 po.ReceivingWarehouseName = "NA";

//             // 🧪 QC Warehouse
//             if (po.QcWarehouseId.HasValue &&
//                 warehouseDtos.TryGetValue(po.QcWarehouseId.Value, out var qcName))
//                 po.QcWarehouseName = qcName;
//             else
//                 po.QcWarehouseName = "NA";
//         }
//                         //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetAll",
//                     actionCode: "GetGrnQCCompletedDetailsHeaderQuery",        
//                     actionName: pendingpoIds.Count.ToString(),
//                     details: $"Pending PO details was fetched.",
//                     module:"GRNEntry"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//                 // ✅ Return
//             return new ApiResponseDTO<List<GetGrnQCCompletedDetailsDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = pendingpoIds,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }
//     }
// }