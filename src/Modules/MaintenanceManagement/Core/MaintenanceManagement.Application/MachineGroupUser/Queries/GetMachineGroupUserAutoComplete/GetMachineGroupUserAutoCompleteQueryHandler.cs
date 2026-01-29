// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
// {
//     public class GetMachineGroupUserAutoCompleteQueryHandler : IRequestHandler<GetMachineGroupUserAutoCompleteQuery, List<MachineGroupUserAutoCompleteDto>>
//     {
//         private readonly IMachineGroupUserQueryRepository _machineGroupQuery;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;

//         public GetMachineGroupUserAutoCompleteQueryHandler(IMachineGroupUserQueryRepository machineGroupQuery, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _machineGroupQuery = machineGroupQuery;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }
//         public async Task<List<MachineGroupUserAutoCompleteDto>> Handle(GetMachineGroupUserAutoCompleteQuery request, CancellationToken cancellationToken)
//         {
//             var result = await _machineGroupQuery.GetMachineGroupUserByName(request.SearchPattern ?? string.Empty);
//             var machineGroupResult = _mapper.Map<List<MachineGroupUserAutoCompleteDto>>(result);

//              // 🔥 Fetch departments using gRPC
//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();

//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var LocationDictionary = new Dictionary<int, MachineGroupUserAutoCompleteDto>();

//             // 🔥 Map department names with DataControl to location
//             foreach (var data in machineGroupResult)
//             {

//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.DepartmentName = departmentName;
//                 }

//                 LocationDictionary[data.DepartmentId] = data;
//             }


//              //Domain Event
//                 var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetAll",
//                     actionCode: "",        
//                     actionName: "",
//                     details: $"MachineGroup User details was fetched.",
//                     module:"MachineGroup User"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//             return machineGroupResult;
//         }
//     }
// }