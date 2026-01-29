// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Commands.Maintenance.PreventiveScheduler;
// using Contracts.Events.Maintenance.PreventiveScheduler;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Application.Common.RealTimeNotificationHub;
// using MaintenanceManagement.Domain.Entities;
// using MassTransit;
// using Microsoft.AspNetCore.SignalR;
// using Newtonsoft.Json;

// namespace MaintenanceManagement.Application.Consumers.PreventiveScheduler
// {
//     public class ScheduleDetailCreateTaskConsumer : IConsumer<CreateShedulerDetailsCommand>
//     {
//         private readonly IPreventiveSchedulerCommand _nextScheduleService;
//         private readonly IMachineMasterQueryRepository _machineMasterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//         private readonly IWorkOrderCommandRepository _workOrderRepository;
//         private readonly IHubContext<PreventiveScheduleHub> _hubContext;
//         private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
//         public ScheduleDetailCreateTaskConsumer(IPreventiveSchedulerCommand nextScheduleService, IMachineMasterQueryRepository machineMasterQueryRepository, IMapper mapper,
//         IMiscMasterQueryRepository miscMasterQueryRepository, IPreventiveSchedulerQuery preventiveSchedulerQuery, IWorkOrderCommandRepository workOrderRepository,
//          IHubContext<PreventiveScheduleHub> hubContext, IPreventiveScheduleLogService preventiveScheduleLogService)
//         {
//             _nextScheduleService = nextScheduleService;
//             _machineMasterQueryRepository = machineMasterQueryRepository;
//             _mapper = mapper;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _workOrderRepository = workOrderRepository;
//             _hubContext = hubContext;
//             _preventiveScheduleLogService = preventiveScheduleLogService;
//         }

//         public async Task Consume(ConsumeContext<CreateShedulerDetailsCommand> context)
//         {
//             //   try
//             // {
//             //     CancellationToken cancellationToken = context.CancellationToken;
//             //     var machineMaster = await _machineMasterQueryRepository.GetMachineByGroupSagaAsync(context.Message.MachineGroupId,context.Message.UnitId);
                
//             //     var details = _mapper.Map<List<PreventiveSchedulerDetail>>(machineMaster);
//             //     var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(context.Message.FrequencyUnitId);
//             //     await _preventiveScheduleLogService.CaptureLogs(context.Message.PreventiveSchedulerHeaderId,null,"Saga Create Schedule Detail",JsonConvert.SerializeObject(details));
//             //         // List<PreventiveSchedulerDetail> list = new List<PreventiveSchedulerDetail>();
//             //         foreach (var detail in details)
//             //          {
                    
//             //                 var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(context.Message.EffectiveDate.ToDateTime(TimeOnly.MinValue), context.Message.FrequencyInterval, frequencyUnit.Code ?? "", context.Message.ReminderWorkOrderDays);
//             //                 var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(context.Message.EffectiveDate.ToDateTime(TimeOnly.MinValue), context.Message.FrequencyInterval, frequencyUnit.Code ?? "", context.Message.ReminderMaterialReqDays);
    
//             //              detail.PreventiveSchedulerHeaderId = context.Message.PreventiveSchedulerHeaderId;
//             //              detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate); 
//             //              detail.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
//             //              detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);
//             //              detail.ScheduleId = context.Message.ScheduleId;
//             //              detail.FrequencyTypeId = context.Message.FrequencyTypeId;
//             //              detail.FrequencyInterval = context.Message.FrequencyInterval;
//             //              detail.FrequencyUnitId = context.Message.FrequencyUnitId;
//             //              detail.GraceDays = context.Message.GraceDays;
//             //              detail.ReminderWorkOrderDays = context.Message.ReminderWorkOrderDays;
//             //              detail.ReminderMaterialReqDays = context.Message.ReminderMaterialReqDays;
//             //              detail.IsDownTimeRequired = context.Message.IsDownTimeRequired;
//             //              detail.DownTimeEstimateHrs = context.Message.DownTimeEstimateHrs;

//             //                  var detailsResponse = await _nextScheduleService.CreateDetailAsync(detail);
//             //             // list.Add(detail);
//             //           }
                     
                    
                       
//             //     if (details.Count > 0)
//             //     {
//             //         await context.Publish(new MachineWiseScheduleCreationEvent
//             //         {
//             //             CorrelationId = context.Message.CorrelationId,
//             //             PreventiveSchedulerHeaderId = context.Message.PreventiveSchedulerHeaderId,
//             //             token = context.Message.token
                        
//             //         });
//             //     }
//             //     else
//             //     {
                     
               
//             //         await context.Publish(new PreventiveSchedulerDetailCreationFailedEvent
//             //         {
//             //             CorrelationId = context.Message.CorrelationId,
//             //             Reason = "Failed to create schedule detail",
//             //             token = context.Message.token
//             //         });
//             //     }
//             // }
//             // catch (Exception ex)
//             // {
                
            
//             //     await context.RespondAsync(new PreventiveSchedulerDetailCreationFailedEvent
//             //     {
//             //         CorrelationId = context.Message.CorrelationId,
//             //         Reason = $"Exception: {ex.Message}",
//             //         token = context.Message.token
//             //     });
//             // }
//         }
//     }
// }