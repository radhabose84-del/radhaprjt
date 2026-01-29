// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MaintenanceManagement.Domain.Entities;
// using MaintenanceManagement.Domain.Events;
// using MediatR;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
// using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
// using static MaintenanceManagement.Domain.Common.MiscEnumEntity;
// using Hangfire;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// // using Core.Application.Common.Interfaces.IBackgroundService;
// using MaintenanceManagement.Application.Common.Interfaces;
// using Contracts.Events.Maintenance.PreventiveScheduler;
// using MaintenanceManagement.Application.Common;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
// using Newtonsoft.Json;
// using Microsoft.AspNetCore.Http;
// using Contracts.Dtos.Maintenance.Preventive;
// using Microsoft.Extensions.Logging;

// namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler
// {
//     public class CreatePreventiveSchedulerCommandHandler : IRequestHandler<CreatePreventiveSchedulerCommand, int>
//     {
//         private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
//         private readonly IHttpContextAccessor _httpContextAccessor;
//         private readonly IMachineMasterQueryRepository _machineMasterQueryRepository;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//         private readonly ILogger<CreatePreventiveSchedulerCommandHandler> _logger;

//         public CreatePreventiveSchedulerCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper, IMediator mediator,
//          IEventPublisher eventPublisher, IIPAddressService iPAddressService, IPreventiveScheduleLogService preventiveScheduleLogService,
//          IHttpContextAccessor httpContextAccessor, IMachineMasterQueryRepository machineMasterQueryRepository, IMiscMasterQueryRepository miscMasterQueryRepository,
//          IPreventiveSchedulerQuery preventiveSchedulerQuery, ILogger<CreatePreventiveSchedulerCommandHandler> logger)
//         {
//             _preventiveSchedulerCommand = preventiveSchedulerCommand;
//             _mapper = mapper;
//             _mediator = mediator;
//             _eventPublisher = eventPublisher;
//             _ipAddressService = iPAddressService;
//             _preventiveScheduleLogService = preventiveScheduleLogService;
//             _httpContextAccessor = httpContextAccessor;
//             _machineMasterQueryRepository = machineMasterQueryRepository;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _logger = logger;

//         }
//         public async Task<int> Handle(CreatePreventiveSchedulerCommand request, CancellationToken cancellationToken)
//         {
            
//             var preventiveScheduler  = _mapper.Map<PreventiveSchedulerHeader>(request);
//             var UnitId = _ipAddressService.GetUnitId();
//              var machineMaster = await _machineMasterQueryRepository.GetMachineByGroupSagaAsync(request.MachineGroupId,UnitId);
                
//                 var details = _mapper.Map<List<PreventiveSchedulerDetail>>(machineMaster);
//                 var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(request.FrequencyUnitId);
                
                    
//                     foreach (var detail in details)
//                      {
                    
//                             var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(request.EffectiveDate.ToDateTime(TimeOnly.MinValue), request.FrequencyInterval, frequencyUnit.Code ?? "", request.ReminderWorkOrderDays);
//                             var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(request.EffectiveDate.ToDateTime(TimeOnly.MinValue), request.FrequencyInterval, frequencyUnit.Code ?? "", request.ReminderMaterialReqDays);
    
                         
//                          detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate); 
//                          detail.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
//                          detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);
//                          detail.ScheduleId = request.ScheduleId;
//                          detail.FrequencyTypeId = request.FrequencyTypeId;
//                          detail.FrequencyInterval = request.FrequencyInterval;
//                          detail.FrequencyUnitId = request.FrequencyUnitId;
//                          detail.GraceDays = request.GraceDays;
//                          detail.ReminderWorkOrderDays = request.ReminderWorkOrderDays;
//                          detail.ReminderMaterialReqDays = request.ReminderMaterialReqDays;
//                          detail.IsDownTimeRequired = request.IsDownTimeRequired;
//                          detail.DownTimeEstimateHrs = request.DownTimeEstimateHrs;

//                       }
//                       preventiveScheduler.PreventiveSchedulerDetails = details;

//                 var response = await _preventiveSchedulerCommand.CreateAsync(preventiveScheduler);
//                 _logger.LogInformation("Created PreventiveSchedulerId: {Id}, Details: {Details}", response.Id, details.Count);

//             // var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
//             if (response.Id > 0 || response != null)
//             {
//                 // var correlationId = Guid.NewGuid();
//                 // var @event = new PreventiveSchedulerHeaderCreationEvent
//                 // {
//                 //     CorrelationId = correlationId,
//                 //     PreventiveSchedulerHeaderId = response,
//                 //     MachineGroupId = preventiveScheduler.MachineGroupId,
//                 //     ScheduleId = preventiveScheduler.ScheduleId,
//                 //     FrequencyTypeId = preventiveScheduler.FrequencyTypeId,
//                 //     FrequencyUnitId = preventiveScheduler.FrequencyUnitId,
//                 //     GraceDays = preventiveScheduler.GraceDays,
//                 //     ReminderWorkOrderDays = preventiveScheduler.ReminderWorkOrderDays,
//                 //     ReminderMaterialReqDays = preventiveScheduler.ReminderMaterialReqDays,
//                 //     IsDownTimeRequired = preventiveScheduler.IsDownTimeRequired,
//                 //     DownTimeEstimateHrs = preventiveScheduler.DownTimeEstimateHrs,
//                 //     EffectiveDate = preventiveScheduler.EffectiveDate,
//                 //     FrequencyInterval = preventiveScheduler.FrequencyInterval,
//                 //     UnitId = UnitId,
//                 //     token = token
//                 // };
//                 // // Save and publish event (RabbitMQ/Saga)
//                 // await _eventPublisher.SaveEventAsync(@event);
//                 // await _eventPublisher.PublishPendingEventsAsync();
//                 var ReverseMapdetails = _mapper.Map<List<ScheduleDetailSagaDto>>(response.PreventiveSchedulerDetails);

//                 var now = DateTime.Now;
//                 var delayById = response.PreventiveSchedulerDetails.ToDictionary(
//                     d => d.Id,
//                     d =>
//                     {
//                         var target = d.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                         var minutes = (int)Math.Ceiling((target - now).TotalMinutes);
//                         return minutes < 0 ? 5 : minutes;
//                     }
//                 );
//                 foreach (var dto in ReverseMapdetails)
//                 {
//                     dto.DelayInMinutes = delayById.TryGetValue(dto.Id, out var mins) ? mins : 5;
//                 }

//                 var correlationId = Guid.NewGuid();
//                 var @event = new MachineWiseScheduleCreationEvent
//                 {
//                     CorrelationId = correlationId,
//                     PreventiveSchedulerHeaderId = response.Id,
//                     ScheduleDetail = ReverseMapdetails
//                 };
//                 // Save and publish event (RabbitMQ/Saga)
//                 await _eventPublisher.SaveEventAsync(@event);
//                 await _eventPublisher.PublishPendingEventsAsync();
                
//                 _logger.LogInformation("Published MachineWiseScheduleCreationEvent for PreventiveSchedulerHeaderId: {Id}", response.Id);
//             }
              
//                      await AuditLogPublisher.PublishAuditLogAsync(
//                      _mediator,
//                      actionDetail: "Create",
//                      actionCode: "NewData",
//                      actionName: "Preventive Schedule Creation",
//                      module: "Preventive",
//                      requestData: request,
//                      cancellationToken
//                     );
               
              
//                  await _preventiveScheduleLogService.CaptureLogs(response.Id,null,"Create Header",JsonConvert.SerializeObject(request));
                 
//                     return response.Id;
            
                
//         }
       
//     }
// }