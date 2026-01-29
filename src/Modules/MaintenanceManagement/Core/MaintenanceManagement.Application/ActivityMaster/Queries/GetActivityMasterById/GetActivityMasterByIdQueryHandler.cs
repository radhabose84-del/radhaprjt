// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
// {
//     public class GetActivityMasterByIdQueryHandler : IRequestHandler<GetActivityMasterByIdQuery, GetActivityMasterByIdDto>
//     {

//         private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//           private readonly IDepartmentGrpcClient _departmentGrpcClient;
//          private readonly IUnitGrpcClient _unitGrpcClient;
//           private readonly IIPAddressService _ipAddressService;
//         public GetActivityMasterByIdQueryHandler(IActivityMasterQueryRepository activityMasterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentGrpcClient departmentGrpcClient, IUnitGrpcClient unitGrpcClient, IIPAddressService ipAddressService)
//         {
//             _activityMasterQueryRepository = activityMasterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentGrpcClient = departmentGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//             _ipAddressService = ipAddressService;
//         }

//         public async Task<GetActivityMasterByIdDto> Handle(GetActivityMasterByIdQuery request, CancellationToken cancellationToken)
//         {

//                var unitId = _ipAddressService.GetUnitId();

//             var result = await _activityMasterQueryRepository.GetByIdAsync(request.Id);

//             // if (result is null)
//             // {
//             //     return new ApiResponseDTO<GetActivityMasterByIdDto>
//             //     {
//             //         IsSuccess = false,
//             //         Message = $"ActivityMaster with Id {request.Id} not found.",
//             //         Data = null
//             //     };
//             // }
//             var activityDto  = _mapper.Map<GetActivityMasterByIdDto>(result);

//               var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//              var units = await _unitGrpcClient.GetAllUnitAsync();
//               var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

//             // Assign department and unit names
//             if (departmentLookup.TryGetValue(activityDto.DepartmentId, out var departmentName))
//                 activityDto.Department = departmentName;

//             if (unitLookup.TryGetValue(activityDto.UnitId, out var unitName))
//                 activityDto.UnitName = unitName;
//             // Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "",
//                 actionName: "",
//                 details: $"ActivityMaster details {activityDto.Id} were fetched.",
//                 module: "ActivityMaster"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             return activityDto;
//         }



//     }
// }