using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MassTransit;
using Newtonsoft.Json;

namespace MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update
{
    public class ScheduleWorkOrderConsumer 
    // : IConsumer<UpdateScheduleWorkOrderCommand>
    {
        // private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        // private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        // private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        // private readonly IBackgroundServiceClient  _backgroundServiceClient;
        // private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        // public ScheduleWorkOrderConsumer(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMiscMasterQueryRepository miscMasterQueryRepository,
        // IPreventiveSchedulerQuery preventiveSchedulerQuery, IBackgroundServiceClient backgroundServiceClient, IPreventiveScheduleLogService preventiveScheduleLogService)
        // {
        //     _preventiveSchedulerCommand = preventiveSchedulerCommand;
        //     _miscMasterQueryRepository = miscMasterQueryRepository;
        //     _preventiveSchedulerQuery = preventiveSchedulerQuery;
        //     _backgroundServiceClient = backgroundServiceClient;
        //     _preventiveScheduleLogService = preventiveScheduleLogService;
        // }

        // public async Task Consume(ConsumeContext<UpdateScheduleWorkOrderCommand> context)
        // {
        //     try
        //     {
                
        //         var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(context.Message.FrequencyUnitId);

        //         var DetailResult = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(context.Message.PreventiveSchedulerHeaderId);
        //          await _preventiveScheduleLogService.CaptureLogs(context.Message.PreventiveSchedulerHeaderId,null,"Saga Update Schedule Details",JsonConvert.SerializeObject(DetailResult));

        //         foreach (var detail in DetailResult)
        //         {
        //             if (context.Message.isFrequencyChanged)
        //             {
        //                 var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate((detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue),
        //                  context.Message.FrequencyInterval, frequencyUnit.Code ?? "", context.Message.ReminderWorkOrderDays);
        //                 var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate((detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue), context.Message.FrequencyInterval, frequencyUnit.Code ?? "", context.Message.ReminderMaterialReqDays);

        //                 detail.PreventiveSchedulerHeaderId = context.Message.PreventiveSchedulerHeaderId;

        //                 detail.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
        //                 detail.FrequencyInterval = context.Message.FrequencyInterval;


        //                 var result = await _preventiveSchedulerQuery.ExistWorkOrderBySchedulerDetailId(detail.Id);
        //                 if (result != true)
        //                 {
        //                     detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate);
        //                     detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);

        //                     // var targetDateTime = detail.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
        //                     // var delay = targetDateTime - DateTime.Now;

        //                     // string newJobId;
        //                     // var delayInMinutes = (int)delay.TotalMinutes;
        //                     // if (delay.TotalSeconds > 0)
        //                     // {

        //                     //     newJobId = await _backgroundServiceClient.ScheduleWorkOrder(detail.Id, delayInMinutes, context.Message.token);
        //                     // }
        //                     // else
        //                     // {

        //                     //     newJobId = await _backgroundServiceClient.ScheduleWorkOrder(detail.Id, 5, context.Message.token);
        //                     // }
        //                     // detail.HangfireJobId = newJobId;
        //                 }

        //             }
        //             else
        //             {
        //                 detail.ReminderWorkOrderDays = context.Message.ReminderWorkOrderDays;
        //                 detail.ReminderMaterialReqDays = context.Message.ReminderMaterialReqDays;
        //             }

        //          }
        //             await _preventiveSchedulerCommand.UpdateScheduleDetails(context.Message.PreventiveSchedulerHeaderId, DetailResult);
                
               
                
        //     }
        //     catch (Exception ex)
        //     {
        //         await context.Publish(new UpdateScheduleWorkOrderFailedEvent
        //         {
        //             CorrelationId = context.Message.CorrelationId,
        //             Reason = "Failed to update schedule detail"
                        
        //             });
        //     }
        // }
    }
}