using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive
{
    public class ReschedulePreventiveCommandHandler : IRequestHandler<ReshedulePreventiveCommand, ApiResponseDTO<bool>>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
         private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly ILogger<ReschedulePreventiveCommandHandler> _logger;
        public ReschedulePreventiveCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper, IMediator mediator,
        IMiscMasterQueryRepository miscMasterQueryRepository, IPreventiveSchedulerQuery preventiveSchedulerQuery, IPreventiveScheduleLogService preventiveScheduleLogService,
        ILogger<ReschedulePreventiveCommandHandler> logger)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _mapper = mapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _logger = logger;
        }
        public async Task<ApiResponseDTO<bool>> Handle(ReshedulePreventiveCommand request, CancellationToken cancellationToken)
        {
            await _preventiveScheduleLogService.CaptureLogs(null,request.PreventiveScheduleDetailId,"Reschedule",JsonConvert.SerializeObject(request));
            
            var result = await _preventiveSchedulerQuery.ExistWorkOrderBySchedulerDetailId(request.PreventiveScheduleDetailId);
            _logger.LogInformation("Reschedule,Is Existing Workorder PreventiveSchedulerId:{PreventiveSchedulerId},IsExistingWorkOrder:{result}", request.PreventiveScheduleDetailId,result);
            
            var closedDate = await _preventiveSchedulerQuery.GetClosedDateBySchedulerDetailId(request.PreventiveScheduleDetailId);
            if (closedDate.HasValue)
            {
                if (request.RescheduleDate == closedDate.Value)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Cannot reschedule to the same date as the completed work order."
                    };
                }
            }

            if (result == true)
            {
                await AuditLogPublisher.PublishAuditLogAsync(
                    _mediator,
                    actionDetail: $"Reschedule Update DueDate",
                    actionCode: "Update DueDate",
                    actionName: "Update DueDate",
                    module: "Preventive",
                    requestData: request,
                    cancellationToken
                   );
                await _preventiveSchedulerCommand.UpdateRescheduleDate(request.PreventiveScheduleDetailId, request.RescheduleDate);
            }
            else
            {
                await AuditLogPublisher.PublishAuditLogAsync(
                    _mediator,
                    actionDetail: $"Reschedule",
                    actionCode: "Reschedule",
                    actionName: "Reschedule",
                    module: "Preventive",
                    requestData: request,
                    cancellationToken
                   );
                await _preventiveSchedulerCommand.RescheduleWithoutWorkOrderAsync(request.PreventiveScheduleDetailId, request.RescheduleDate, cancellationToken);

            }

          
                return new ApiResponseDTO<bool>()
                {
                    IsSuccess = true,
                    Message = "Preventive Scheduler Rescheduled successfully."
                };
            
            
        }
    }
}