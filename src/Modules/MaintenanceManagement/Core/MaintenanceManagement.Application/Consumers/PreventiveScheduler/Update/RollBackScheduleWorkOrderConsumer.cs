// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Commands.Maintenance.PreventiveScheduler.Update;
// using Contracts.Interfaces.External.IMaintenance;
// using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
// using MaintenanceManagement.Domain.Entities;
// using MassTransit;

// namespace MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update
// {
//     public class RollBackScheduleWorkOrderConsumer : IConsumer<RollBackScheduleWorkOrderCommand>
//     {
//         private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
//         private readonly IMapper _mapper;
//         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//         private readonly IBackgroundServiceClient  _backgroundServiceClient;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
//         public RollBackScheduleWorkOrderConsumer(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper,
//         IPreventiveSchedulerQuery preventiveSchedulerQuery, IBackgroundServiceClient backgroundServiceClient, IMiscMasterQueryRepository miscMasterQueryRepository,
//         IPreventiveScheduleLogService preventiveScheduleLogService)
//         {
//             _preventiveSchedulerCommand = preventiveSchedulerCommand;
//             _mapper = mapper;
//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _backgroundServiceClient = backgroundServiceClient;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _preventiveScheduleLogService = preventiveScheduleLogService;
//         }
//         public async Task Consume(ConsumeContext<RollBackScheduleWorkOrderCommand> context)
//         {
//             await _preventiveScheduleLogService.CaptureLogs(context.Message.rollbackHeaders.Id, null, "Update Schedule Roll Back", context.Message.Reason);
//             var rollbackHeader = _mapper.Map<PreventiveSchedulerHeader>(context.Message.rollbackHeaders);
//             await _preventiveSchedulerCommand.UpdateScheduleMetadata(rollbackHeader);

//             // var DetailResult = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(context.Message.PreventiveSchedulerHeaderId);

//             // var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(rollbackHeader.FrequencyUnitId);
//             var rollbackDetails = new List<PreventiveSchedulerDetail>();
//             foreach (var detail in context.Message.rollbackScheduleDetail)
//             {
                
//                     var dto = _mapper.Map<PreventiveSchedulerDetail>(detail);
//                     rollbackDetails.Add(dto);
                
//             }
//             await _preventiveSchedulerCommand.UpdateScheduleDetails(context.Message.rollbackHeaders.Id, rollbackDetails);
            
           
//         }
//     }
// }