#nullable disable

using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule
{
    public class CreateWOScheduleCommandHandler : IRequestHandler<CreateWOScheduleCommand, ApiResponseDTO<bool>>
    { 
        private readonly IWorkOrderCommandRepository _workOrderRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ITimeZoneService _timeZoneService;

        public CreateWOScheduleCommandHandler(IWorkOrderCommandRepository workOrderRepository, IMapper mapper, IMediator mediator, ITimeZoneService timeZoneService)
        {
            _workOrderRepository = workOrderRepository;
            _mapper = mapper;
            _mediator = mediator;
            _timeZoneService = timeZoneService; 
        }

        public async Task<ApiResponseDTO<bool>> Handle(CreateWOScheduleCommand request, CancellationToken cancellationToken)
        {   
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
                request.WOSchedule.StartTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, systemTz);                
                if (request.WOSchedule.EndTime.HasValue)
                {
                    request.WOSchedule.EndTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, systemTz);                    
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
            /* 
            request.WOSchedule.StartTime=request.WOSchedule.StartTime;
            if (request.WOSchedule.EndTime != null)
            {
                //request.WOSchedule.EndTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, systemTimeZone);
                request.WOSchedule.EndTime = request.WOSchedule.EndTime.Value;
            }            
       */
            var createWOEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>(request.WOSchedule);                   
            var updateResult = await _workOrderRepository.CreateScheduleAsync(createWOEntity.WorkOrderId, createWOEntity);            
        
            //Domain Event 
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: request.WOSchedule.WorkOrderId.ToString(),
                actionName: "",                            
                details: $"WorkOrder Schedule '{request.WOSchedule.WorkOrderId}' was updated",
                module:"WorkOrderSchedule Create"
            );            
            await _mediator.Publish(domainEvent, cancellationToken);
            if(updateResult!=0)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "WorkOrder Schedule inserted successfully.",                        
                };
            }
            return new ApiResponseDTO<bool>
            {
                IsSuccess = false,
                Message = "WorkOrder Schedule not inserted."
            };                
        }          
    }
 }
