// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser
// {
//     public class GetMachineGroupUserQueryHandler : IRequestHandler<GetMachineGroupUserQuery, ApiResponseDTO<List<MachineGroupUserDto>>>
//     {
//         private readonly IMachineGroupUserQueryRepository _machineGroupQuery;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;


//         public GetMachineGroupUserQueryHandler(IMachineGroupUserQueryRepository machineGroupQuery, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _machineGroupQuery = machineGroupQuery;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;

//         }
//         public async Task<ApiResponseDTO<List<MachineGroupUserDto>>> Handle(GetMachineGroupUserQuery request, CancellationToken cancellationToken)
//         {
//             var (machineGroupUser, totalCount) = await _machineGroupQuery.GetAllMachineGroupUserAsync(request.PageNumber, request.PageSize, request.SearchTerm);
//             var machineGroupUserList = _mapper.Map<List<MachineGroupUserDto>>(machineGroupUser);

//             // 🔥 Fetch departments using gRPC
//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();

//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var LocationDictionary = new Dictionary<int, MachineGroupUserDto>();

//             // 🔥 Map department names with DataControl to location
//             foreach (var data in machineGroupUserList)
//             {

//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.DepartmentName = departmentName;
//                 }

//                 LocationDictionary[data.DepartmentId] = data;
//             }

//             //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetMachineGroupUser",
//                     actionCode: "",
//                     actionName: "",
//                     details: $"MachineGroupUser details was fetched.",
//                     module: "MachineGroupUser"
//                 );
//             await _mediator.Publish(domainEvent, cancellationToken);
//             return new ApiResponseDTO<List<MachineGroupUserDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = machineGroupUserList,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }
//     }
// }