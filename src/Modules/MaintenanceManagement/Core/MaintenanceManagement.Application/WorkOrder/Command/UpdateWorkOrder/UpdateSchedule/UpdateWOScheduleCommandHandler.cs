
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule
{
    public class UpdateWOScheduleCommandHandler : IRequestHandler<UpdateWOScheduleCommand, ApiResponseDTO<bool>>
    { 
        private readonly IWorkOrderCommandRepository _workOrderRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ITimeZoneService _timeZoneService; 

        public UpdateWOScheduleCommandHandler(IWorkOrderCommandRepository workOrderRepository, IMapper mapper,IMediator mediator, ITimeZoneService timeZoneService)
        {
            _workOrderRepository = workOrderRepository;
            _mapper = mapper;            
            _mediator = mediator;
            _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdateWOScheduleCommand request, CancellationToken cancellationToken)
        {       
             if (request.WOSchedule == null)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "Schedule data is missing."
                };
            }   
            var tzId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTz;
            try
            {
                systemTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch
            {
                systemTz = TimeZoneInfo.Local;
            }

            if (request.WOSchedule.IsSystemTime == 1)
            {                
                if (request.WOSchedule.EndTime.HasValue)
                {
                    request.WOSchedule.EndTime =
                        TimeZoneInfo.ConvertTime(DateTime.UtcNow, systemTz);
                }
            }  
            else
            {
                request.WOSchedule.StartTime = TimeZoneInfo.ConvertTime(request.WOSchedule.StartTime, systemTz);
                if (request.WOSchedule.EndTime.HasValue)
                {
                    request.WOSchedule.EndTime =
                        TimeZoneInfo.ConvertTime(request.WOSchedule.EndTime.Value, systemTz);
                }
            }                  

            var updatedWOEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>(request.WOSchedule);                   
            var updateResult = await _workOrderRepository.UpdateScheduleAsync(updatedWOEntity.WorkOrderId, updatedWOEntity);            
        
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: request.WOSchedule.WorkOrderId.ToString(),
                actionName: "",                            
                details: $"WorkOrder Schedule '{request.WOSchedule.WorkOrderId}' was updated",
                module:"WorkOrderSchedule Update"
            );            
            await _mediator.Publish(domainEvent, cancellationToken);
            if(updateResult)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "WorkOrder Schedule Updated successfully.",                        
                };
            }
            return new ApiResponseDTO<bool>
            {
                IsSuccess = false,
                Message = "WorkOrder Schedule not Updated."
            };                
        }          
    }
 }
