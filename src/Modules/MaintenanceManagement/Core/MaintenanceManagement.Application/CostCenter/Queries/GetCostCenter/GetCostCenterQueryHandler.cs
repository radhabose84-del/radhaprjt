// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter
// {
//     public class GetCostCenterQueryHandler : IRequestHandler<GetCostCenterQuery, ApiResponseDTO<List<CostCenterDto>>>
//     {
//         private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient; // 👈 gRPC Inject here
//         private readonly IUnitGrpcClient _unitGrpcClient; // 👈 gRPC Inject here


//         public GetCostCenterQueryHandler(ICostCenterQueryRepository iCostCenterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient, IUnitGrpcClient unitGrpcClient)
//         {
//             _iCostCenterQueryRepository = iCostCenterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<CostCenterDto>>> Handle(GetCostCenterQuery request, CancellationToken cancellationToken)
//         {
//             var (costCenters, totalCount) = await _iCostCenterQueryRepository.GetAllCostCenterGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
//             var costCenterDtos = _mapper.Map<List<CostCenterDto>>(costCenters);

//             // 🔥 Fetch departments using gRPC
//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var units = await _unitGrpcClient.GetAllUnitAsync();

//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
//             var costcenterDictionary = new Dictionary<int, CostCenterDto>();



//             // 🔥 Map department & unit names with DataControl to costCenters
//             foreach (var data in costCenterDtos)
//             {

//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.DepartmentName = departmentName;
//                 }
//                 if (unitLookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
//                 {
//                     data.UnitName = unitName;
//                 }

//                 costcenterDictionary[data.UnitId] = data;

//             }
//             // foreach (var dto in costCenterDtos)
//             // {
//             //     if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
//             //         dto.DepartmentName = deptName;

//             //     if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
//             //         dto.UnitName = unitName;
//             // }

//             // 📘 Log domain event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetCostCenter",
//                 actionCode: "Get",
//                 actionName: costCenters.Count().ToString(),
//                 details: "CostCenter details were fetched.",
//                 module: "CostCenter"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             // ✅ Return
//             return new ApiResponseDTO<List<CostCenterDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = costCenterDtos,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }



//     }
// }