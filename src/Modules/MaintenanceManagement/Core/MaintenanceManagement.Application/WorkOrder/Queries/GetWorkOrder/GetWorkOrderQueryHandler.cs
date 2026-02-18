
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder
{
    public class GetWorkOrderQueryHandler : IRequestHandler<GetWorkOrderQuery, ApiResponseDTO<List<Dictionary<string, List<GetWorkOrderDto>>>>>
    {
        private readonly IWorkOrderQueryRepository _workOrderRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IDepartmentLookup _departmentLookup;

        public GetWorkOrderQueryHandler(IWorkOrderQueryRepository workOrderRepository, IMapper mapper, IMediator mediator,
         IDepartmentLookup departmentLookup)
        {
            _workOrderRepository = workOrderRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }
  
        public async Task<ApiResponseDTO<List<Dictionary<string, List<GetWorkOrderDto>>>>> Handle(GetWorkOrderQuery request, CancellationToken cancellationToken)
        {
           var workOrder = await _workOrderRepository.GetAllWOAsync(request.fromDate,request.toDate, request.requestTypeId, request.departmentId,request.machineId);            
           var mappedWorkOrders = _mapper.Map<List<GetWorkOrderDto>>(workOrder);

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var filteredWorkOrders = mappedWorkOrders
                 .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
                 .Select(p =>
                 {
                     p.Department = departmentLookup[p.DepartmentId];
                     return p;
                 })
                 .ToList();

            var groupedWorkOrders = filteredWorkOrders
            .GroupBy(w => w.MaintenanceType ?? "Unknown")
            .Select(g => new Dictionary<string, List<GetWorkOrderDto>>
            {
                [g.Key] = g.ToList()
            })
            .ToList();

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"WorkOrderDetails details was fetched.",
                module:"WorkOrderDetails"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
           return new ApiResponseDTO<List<Dictionary<string, List<GetWorkOrderDto>>>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = groupedWorkOrders,          
            };     
        }
    }
}
