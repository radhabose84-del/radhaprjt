// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
// using MaintenanceManagement.Domain.Events;
// using MassTransit;
// using MediatR;

// namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster
// {
//     public class GetAllActivityCheckListMasterQueryHandler : IRequestHandler<GetAllActivityCheckListMasterQuery, ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>>
//     {


//          private readonly IActivityCheckListMasterQueryRepository _activityCheckListMasterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentGrpcClient _departmentGrpcClient;
//         private readonly  IUnitGrpcClient _unitGrpcClient;
//         private readonly IIPAddressService _ipAddressService;


//         public GetAllActivityCheckListMasterQueryHandler(IActivityCheckListMasterQueryRepository activityCheckListMasterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentGrpcClient departmentGrpcClient, IUnitGrpcClient unitGrpcClient, IIPAddressService ipAddressService)
//         {
//             _activityCheckListMasterQueryRepository = activityCheckListMasterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentGrpcClient = departmentGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//             _ipAddressService = ipAddressService;
//         }



//         public async Task<ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>> Handle(GetAllActivityCheckListMasterQuery request, CancellationToken cancellationToken)
//         {
//             // Fetch data from repository
//             var (checkLists, totalCount) = await _activityCheckListMasterQueryRepository.GetAllActivityCheckListMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);

//             // Map domain entities to DTOs
//             var checkListDto = _mapper.Map<List<GetAllActivityCheckListMasterDto>>(checkLists); 


//               // 🔥 Fetch departments using gRPC
//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);


//             var activityMasterDictionary = new Dictionary<int, GetAllActivityCheckListMasterDto>();

//             var units = await _unitGrpcClient.GetAllUnitAsync();
//             var unitslookup = units.ToDictionary(d => d.UnitId, d => d.UnitName);                                               
           
//              foreach (var data in checkListDto)
//             {

//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {

//                     data.DepartmentName = departmentName;
//                 }
              
//                 if (unitslookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
//                 {
//                     data.UnitName = unitName;
//                 }
             
               
//             }

//             var filteredActivities = checkListDto
//             .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
//             .ToList();          

//             // Publish domain event for auditing
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "",
//                 actionName: "",
//                 details: "Activity Checklist Master details were fetched.",
//                 module: "ActivityCheckListMaster"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             // Return API response
//             return new ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = filteredActivities,
//                 TotalCount = filteredActivities.Count,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }
//     }
// }