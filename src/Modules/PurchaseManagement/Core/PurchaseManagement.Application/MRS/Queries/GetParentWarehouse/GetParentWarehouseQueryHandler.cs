// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IWarehouse;
// using PurchaseManagement.Application.Common.Interfaces.IMRS;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.MRS.Queries.GetParentWarehouse
// {
//     public class GetParentWarehouseQueryHandler : IRequestHandler<GetParentWarehouseQuery, GetParentWarehouseDto>
//     {
        
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;
//         private readonly IMediator _mediator;

//         public GetParentWarehouseQueryHandler(IWarehouseGrpcClient warehouseGrpcClient, IMediator mediator)
//         {

//             _warehouseGrpcClient = warehouseGrpcClient;
//             _mediator = mediator;
//         }
 
//         public async Task<GetParentWarehouseDto> Handle(GetParentWarehouseQuery request, CancellationToken cancellationToken)
//         {
//              // 1️⃣ Get the current warehouse by Id (example: 59)
//             var currentWarehouse = await _warehouseGrpcClient.GetByIdAsync(request.WarehouseId, cancellationToken);

//             if (currentWarehouse == null)
//             {
//                 return new GetParentWarehouseDto
//                 {
//                     ParentWarehouseId = 0,
//                     ParentWarehouseName = "Warehouse Not Found"
//                 };
//             }

//             // 2️⃣ If no parent warehouse exists
//             if (currentWarehouse.ParentWarehouseId <= 0)
//             {
//                 return new GetParentWarehouseDto
//                 {
//                     ParentWarehouseId = 0,
//                     ParentWarehouseName = "No Parent Warehouse"
//                 };
//             }

//             // 3️⃣ Fetch the actual parent warehouse (example: Id = 55)
//             var parentWarehouseTask = _warehouseGrpcClient.GetByIdAsync(currentWarehouse.ParentWarehouseId, cancellationToken);
//             await Task.WhenAll(parentWarehouseTask);

//             var parentWarehouse = parentWarehouseTask.Result;

//             // 4️⃣ Map the parent warehouse details
//             var result = new GetParentWarehouseDto
//             {
//                 ParentWarehouseId = parentWarehouse?.Id ?? 0,
//                 ParentWarehouseName = parentWarehouse?.WarehouseName ?? "Parent Warehouse Not Found"
//             };

//             // 5️⃣ Optional audit event for tracking
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "GetParentWarehouseQuery",
//                 actionName: $"ParentWarehouseId: {result.ParentWarehouseId}",
//                 details: $"Fetched parent warehouse for WarehouseId {request.WarehouseId}",
//                 module: "Warehouse"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             return result;
//         }
//     }
// }