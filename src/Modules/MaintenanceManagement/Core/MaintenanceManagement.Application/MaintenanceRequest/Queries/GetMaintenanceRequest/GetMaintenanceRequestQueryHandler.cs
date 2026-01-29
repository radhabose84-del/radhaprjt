// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest
// {
//     public class GetMaintenanceRequestQueryHandler : IRequestHandler<GetMaintenanceRequestQuery, ApiResponseDTO<List<GetMaintenanceRequestDto>>>
//     {
//         private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;

//         private readonly IDepartmentGrpcClient _departmentGrpcClient;

//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;


//         public GetMaintenanceRequestQueryHandler(
//             IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
//             IMapper mapper,
//             IMediator mediator,
//             IDepartmentGrpcClient departmentService,
//             IUsersAllGrpcClient usersAllGrpcClient)

//         {
//             _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentGrpcClient = departmentService;
//             _usersAllGrpcClient = usersAllGrpcClient;


//         }

//         public async Task<ApiResponseDTO<List<GetMaintenanceRequestDto>>> Handle(GetMaintenanceRequestQuery request, CancellationToken cancellationToken)
//         {
//             var (maintenanceRequests, totalCount) = await _maintenanceRequestQueryRepository.GetAllMaintenanceRequestAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
//             var maintenanceRequestList = _mapper.Map<List<GetMaintenanceRequestDto>>(maintenanceRequests);

//             // 🔥 Fetch departments using gRPC
//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync(); // ✅ Clean call
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//             var maintenanceRequestDictionary = new Dictionary<int, GetMaintenanceRequestDto>();
            
//            //🔥 Fetch users using gRPC
//             var users = await _usersAllGrpcClient.GetUserAllAsync();
//             var userLookup = users.ToDictionary(d => d.UserId, d => d.FirstName + " " + d.LastName);

//             // 🔥 Map department names with DataControl to location

//             foreach (var data in maintenanceRequestList)
//             {

//                 if (departmentLookup.TryGetValue(data.ProductionDepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.ProductionDepartmentName = departmentName;
//                 }
//                 maintenanceRequestDictionary[data.ProductionDepartmentId] = data;

//                 if (userLookup.TryGetValue(data.CreatedBy, out var userName) && userName != null)
//                 {
//                     data.CreatedUsername = userName;
//                  }
//             }

//                var filteredMaintenanceRequest = maintenanceRequestList
//             .Where(p => departmentLookup.ContainsKey(p.ProductionDepartmentId))
//             .ToList();
            
//             // var filteredmaintenanceRequestDtos = maintenanceRequestList
//             //  .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
//             //  .Select(p => new GetMaintenanceRequestDto
//             //  {
//             //      DepartmentId = p.DepartmentId,
//             //      DepartmentName = departmentLookup[p.DepartmentId],
//             //  })
//             //  .ToList();

//             // Domain Event Logging
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "",
//                 actionName: "",
//                 details: "MaintenanceRequest records were fetched.",
//                 module: "MaintenanceRequest"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             return new ApiResponseDTO<List<GetMaintenanceRequestDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = filteredMaintenanceRequest,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }
//     }
// }