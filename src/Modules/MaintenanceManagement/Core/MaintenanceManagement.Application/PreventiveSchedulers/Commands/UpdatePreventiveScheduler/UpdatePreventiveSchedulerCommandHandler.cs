// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Maintenance.Preventive;
// using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
// using Contracts.Interfaces.External.IMaintenance;
// using MaintenanceManagement.Application.Common;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// // using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
// using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
// using MaintenanceManagement.Domain.Entities;
// using MaintenanceManagement.Domain.Events;
// using Hangfire;
// using MediatR;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using static MaintenanceManagement.Domain.Common.MiscEnumEntity;

// namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler
// {
//     public class UpdatePreventiveSchedulerCommandHandler : IRequestHandler<UpdatePreventiveSchedulerCommand, ApiResponseDTO<bool>>
//     {
//         private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
        
//         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//         private readonly IWorkOrderCommandRepository _workOrderRepository;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly ITimeZoneService _timeZoneService;
        
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
//         private readonly IHttpContextAccessor _httpContextAccessor;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly ILogger<UpdatePreventiveSchedulerCommandHandler> _logger;
//         public UpdatePreventiveSchedulerCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper, IMediator mediator,
//          IPreventiveSchedulerQuery preventiveSchedulerQuery, IWorkOrderCommandRepository workOrderRepository,
//         IIPAddressService ipAddressService, ITimeZoneService timeZoneService, IEventPublisher eventPublisher, IPreventiveScheduleLogService preventiveScheduleLogService,
//         IHttpContextAccessor httpContextAccessor, IMiscMasterQueryRepository miscMasterQueryRepository, ILogger<UpdatePreventiveSchedulerCommandHandler> logger)
//         {
//             _preventiveSchedulerCommand = preventiveSchedulerCommand;
//             _mapper = mapper;
//             _mediator = mediator;

//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _workOrderRepository = workOrderRepository;
//             _ipAddressService = ipAddressService;
//             _timeZoneService = timeZoneService;

//             _eventPublisher = eventPublisher;
//             _preventiveScheduleLogService = preventiveScheduleLogService;
//             _httpContextAccessor = httpContextAccessor;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _logger = logger;
//         }
//         public async Task<ApiResponseDTO<bool>> Handle(UpdatePreventiveSchedulerCommand request, CancellationToken cancellationToken)
//         {
//            await _preventiveScheduleLogService.CaptureLogs(request.Id,null,"Update",JsonConvert.SerializeObject(request));
//             var preventiveScheduler  = _mapper.Map<PreventiveSchedulerHeader>(request);

//             var existingPreventiveScheduler = await _preventiveSchedulerQuery.GetByIdAsync(request.Id);
            
//             var rollbackHeader = _mapper.Map<RollbackHeaderDto>(existingPreventiveScheduler);
            
//             bool isFrequencyChanged =
//             request.FrequencyInterval != existingPreventiveScheduler.FrequencyInterval ||
//             request.FrequencyTypeId != existingPreventiveScheduler.FrequencyTypeId ||
//             request.FrequencyUnitId != existingPreventiveScheduler.FrequencyUnitId;

//             _logger.LogInformation("Is Frequency Changed PreventiveSchedulerId:{PreventiveSchedulerId},isFrequencyChanged:{isFrequencyChanged}", request.Id,isFrequencyChanged);

//             var metaDataResponse = await _preventiveSchedulerCommand.UpdateScheduleMetadata(preventiveScheduler);

//             if (metaDataResponse != null && metaDataResponse.Id > 0)
//             {
//                 var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(metaDataResponse.FrequencyUnitId);

//                 var DetailResult = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(metaDataResponse.Id);
//                 //  await _preventiveScheduleLogService.CaptureLogs(metaDataResponse.Id,null,"Saga Update Schedule Details",JsonConvert.SerializeObject(DetailResult));
//                 // var rollbackDetail = _mapper.Map<ScheduleDetailSagaDto>(DetailResult);
//                 var rollbackDetails = new List<RollbackScheduleDetailDto>();
//                 var hangfireScheduleDetail = new List<ScheduleDetailUpdateDto>();

//                 foreach (var detail in DetailResult)
//                 {
//                     var dto = _mapper.Map<RollbackScheduleDetailDto>(detail);
                    
//                     if (isFrequencyChanged)
//                     {
//                         var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate((detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue),
//                          metaDataResponse.FrequencyInterval, frequencyUnit.Code ?? "", metaDataResponse.ReminderWorkOrderDays);
//                         var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate((detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue), metaDataResponse.FrequencyInterval, frequencyUnit.Code ?? "", metaDataResponse.ReminderMaterialReqDays);

//                         detail.PreventiveSchedulerHeaderId = metaDataResponse.Id;

//                         detail.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
//                         detail.FrequencyInterval = metaDataResponse.FrequencyInterval;


//                         var result = await _preventiveSchedulerQuery.ExistWorkOrderBySchedulerDetailId(detail.Id);
//                         _logger.LogInformation("Existing Work Order PreventiveSchedulerId:{PreventiveSchedulerId},IsExisting:{result}", request.Id, result);
//                         if (result != true)
//                         {

//                               rollbackDetails.Add(dto);


//                             detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate);
//                             detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);

//                             // var targetDateTime = detail.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                             // var delay = targetDateTime - DateTime.Now;

//                             // var delayInMinutes = (int)delay.TotalMinutes < 0 ? 5 : (int)delay.TotalMinutes;
//                             var scheduleDetailUpdateDto = _mapper.Map<ScheduleDetailUpdateDto>(detail);
//                             // scheduleDetailUpdateDto.DelayInMinutes = delayInMinutes;
//                             hangfireScheduleDetail.Add(scheduleDetailUpdateDto);


//                         }
                       


//                     }
//                     else
//                     {
                      

//                         rollbackDetails.Add(dto);

//                         detail.ReminderWorkOrderDays = metaDataResponse.ReminderWorkOrderDays;
//                         detail.ReminderMaterialReqDays = metaDataResponse.ReminderMaterialReqDays;

//                     }


                    

//                 }
//                  await _preventiveSchedulerCommand.UpdateScheduleDetails(metaDataResponse.Id, DetailResult);
              
//                 // var scheduleDetailUpdateDto = _mapper.Map<List<ScheduleDetailUpdateDto>>(Updatedresult);
             
//                  var correlationId = Guid.NewGuid();
//                             var @event = new HeaderUpdateEvent
//                             {
//                                 CorrelationId = correlationId,
//                                 ScheduleDetailUpdate = hangfireScheduleDetail,
//                                 rollbackHeaders = rollbackHeader,
//                                 rollbackDetails = rollbackDetails

//                             };

//                             await _eventPublisher.SaveEventAsync(@event);
//                             await _eventPublisher.PublishPendingEventsAsync();

//                             _logger.LogInformation("Schedule Work Order Published PreventiveSchedulerId:{PreventiveSchedulerId}", request.Id);

//                 //  var ReverseMapdetails = _mapper.Map<List<ScheduleDetailSagaDto>>(DetailResult);

//                 // var now = DateTime.Now;
//                 // var delayById = DetailResult.ToDictionary(
//                 //     d => d.Id,
//                 //     d =>
//                 //     {
//                 //         var target = d.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                 //         var minutes = (int)Math.Ceiling((target - now).TotalMinutes);
//                 //         return minutes < 0 ? 5 : minutes;
//                 //     }
//                 // );
//                 // var UnitId = _ipAddressService.GetUnitId();



//             }

//               await AuditLogPublisher.PublishAuditLogAsync(
//                      _mediator,
//                      actionDetail: $"Schedule Update request",
//                      actionCode: "Schedule Update",
//                      actionName: "Schedule Update",
//                      module: "Preventive",
//                      requestData: request,
//                      cancellationToken
//                     );
                 
              
//                 if(metaDataResponse.Id > 0)
//                 {
//                     return new ApiResponseDTO<bool>
//                     {
//                         IsSuccess = true, 
//                         Message = "Preventive Scheduler updated successfully."
//                     };
//                 }

//                 return new ApiResponseDTO<bool>
//                 {
//                     IsSuccess = false, 
//                     Message = "Preventive Scheduler not updated."
//                 };
//         }
//     }
// }