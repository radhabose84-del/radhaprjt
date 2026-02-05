// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWarehouse;
// using InventoryManagement.Application.Common.HttpResponse;
// using InventoryManagement.Application.Common.Interfaces.IIssue;
// using InventoryManagement.Domain.Events;
// using MediatR;

// namespace InventoryManagement.Application.Issue.Queries.GetPendingIssueHeader
// {
//     public class GetPendingIssueHeaderQueryHandler : IRequestHandler<GetPendingIssueHeaderQuery, ApiResponseDTO<List<GetPendingIssueHeaderDto>>>
//     {
//         private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;
//          public GetPendingIssueHeaderQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator, IUnitGrpcClient unitGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IWarehouseGrpcClient warehouseGrpcClient)
//         {
//             _iissueQueryCommandRepository = iissueQueryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _unitGrpcClient = unitGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _warehouseGrpcClient = warehouseGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<GetPendingIssueHeaderDto>>> Handle(GetPendingIssueHeaderQuery request, CancellationToken cancellationToken)
//         {
//             var (result, totalCount) = await _iissueQueryCommandRepository.GetPendingIssueHeaderAsync(
//             request.FromDate,
//             request.ToDate,
//             request.PageNumber,
//             request.PageSize,
//             request.SearchTerm
//         );

//             var pendingGrnList = _mapper.Map<List<GetPendingIssueHeaderDto>>(result);
//             var warehouseIds = result.Select(r => r.SubStoresWarehouseId)
//                                .Where(id => id > 0)
//                                .Distinct()
//                                .ToList();
//              // 3️⃣ Fire parallel gRPC calls
//             var unitTask = _unitGrpcClient.GetAllUnitAsync();
//             var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var warehouseTasks = warehouseIds.Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();
//              // Wait for basics first
//             await Task.WhenAll(unitTask, departmentTask);
//             var warehouseResults = await Task.WhenAll(warehouseTasks);
//             // 4️⃣ Prepare lookup dictionaries
//             var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
//             var departmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//             var warehouseLookup = warehouseResults
//                 .Where(w => w != null)
//                 .ToDictionary(w => w.Id, w => w.WarehouseName);


//             // 5️⃣ Process and enrich each record
//             foreach (var dto in pendingGrnList)
//             {
//                 if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
//                     dto.UnitName = unitName;

//                 if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
//                     dto.DepartmentName = deptName;

//                 if (departmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
//                     dto.SubDepartmentName = subDeptName;

//                 if (warehouseLookup.TryGetValue(dto.SubStoresWarehouseId, out var subWarehouseName))
//                     dto.SubStoresWarehouseName = subWarehouseName;
//             }

//             // 5) Domain event (unchanged)
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "GetPendingIssueHeaderQuery",
//                 actionName: pendingGrnList.Count.ToString(),
//                 details: "Issue Pending details were fetched",
//                 module: "IssueEntry");

//             await _mediator.Publish(domainEvent, cancellationToken);

//             return new ApiResponseDTO<List<GetPendingIssueHeaderDto>>
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