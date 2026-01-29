
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
// {
//     public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, ApiResponseDTO<GetWorkOrderByIdDto>>
//     {
//         private readonly IWorkOrderQueryRepository _workOrderQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;   
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IWorkOrderCommandRepository _workOrderRepository;     
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient; 

//         public GetWorkOrderByIdQueryHandler(IWorkOrderQueryRepository workOrderQueryRepository,  IMapper mapper, IMediator mediator, IIPAddressService ipAddressService,IWorkOrderCommandRepository workOrderRepository,  IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _workOrderQueryRepository =workOrderQueryRepository;
//             _mapper =mapper;
//             _mediator = mediator;           
//             _ipAddressService = ipAddressService;
//             _workOrderRepository = workOrderRepository; 
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }
//         public async Task<ApiResponseDTO<GetWorkOrderByIdDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
//         {          
//             var (woResult, woActivity, woItem,woTechnician,woCheckList,woSchedule) = await _workOrderQueryRepository.GetWorkOrderByIdAsync(request.Id);
//             if (woResult == null)
//             {
//                 return new ApiResponseDTO<GetWorkOrderByIdDto>
//                 {
//                     IsSuccess = false,
//                     Message = $"WorkOrder with ID {request.Id} not found."
//                 };
//             }

//             var mappedWorkOrders = _mapper.Map<GetWorkOrderByIdDto>(woResult);      

//              // 🔥 Fetch departments using gRPC
//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync(); // ✅ Clean call

//             // var departments = departmentResponse.Departments.ToList();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//            if (!departmentLookup.TryGetValue(mappedWorkOrders.DepartmentId, out string departmentName))
//             {
//                 return new ApiResponseDTO<GetWorkOrderByIdDto>
//                 {
//                     IsSuccess = false,
//                     Message = "No workorder details found"
//                     //Data = new GetWorkOrderByIdDto() // Return an empty DTO object
//                 };
//             }
//             mappedWorkOrders.Department = departmentName;
//             if (woActivity != null)
//             {
//                 mappedWorkOrders.WOActivity  = _mapper.Map<List<GetWorkOrderActivityByIdDto>>(woActivity);
//             }
//             if (woItem != null)
//             {
//                 mappedWorkOrders.WOItem  = _mapper.Map<List<GetWorkOrderItemByIdDto>>(woItem);             
//             }
//             if (woTechnician != null)
//             {
//                 mappedWorkOrders.WOTechnician  = _mapper.Map<List<GetWorkOrderTechnicianByIdDto>>(woTechnician);
//             }       
//             if (woSchedule != null)
//             {
//                 mappedWorkOrders.WOSchedule  = _mapper.Map<List<GetWorkOrderScheduleByIdDto>>(woSchedule);
//             }       
//             if (woCheckList != null)
//             {
//                 mappedWorkOrders.WOCheckList  = _mapper.Map<List<GetWorkOrderCheckListByIdDto>>(woCheckList);
//             }           
         
//             //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode:"",        
//                 actionName: "",                
//                 details: $"mappedWorkOrders ",
//                 module:"WorkOrder"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);
//             return new ApiResponseDTO<GetWorkOrderByIdDto>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = mappedWorkOrders
//             };       
//         }      
//     }
// }