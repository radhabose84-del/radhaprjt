using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine
{
    public class MapMachineCommandHandler : IRequestHandler<MapMachineCommand, ApiResponseDTO<bool>>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        private readonly IMapper _mapper;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IBackgroundServiceClient _backgroundServiceClient;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MapMachineCommandHandler> _logger;

        public MapMachineCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper,
            IPreventiveSchedulerQuery preventiveSchedulerQuery, IMiscMasterQueryRepository miscMasterQueryRepository,
            IBackgroundServiceClient backgroundServiceClient, IPreventiveScheduleLogService preventiveScheduleLogService,
            IHttpContextAccessor httpContextAccessor, ILogger<MapMachineCommandHandler> logger)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _mapper = mapper;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _backgroundServiceClient = backgroundServiceClient;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<bool>> Handle(MapMachineCommand request, CancellationToken cancellationToken)
        {
            await _preventiveScheduleLogService.CaptureLogs(request.Id, null, "Link Machine", JsonConvert.SerializeObject(request));
            var PreventiveSchedule = await _preventiveSchedulerQuery.GetByIdAsync(request.Id);
            var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(PreventiveSchedule.FrequencyUnitId);

            var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(request.LastMaintenanceActivityDate.ToDateTime(TimeOnly.MinValue), PreventiveSchedule.FrequencyInterval, frequencyUnit.Code ?? "", PreventiveSchedule.ReminderWorkOrderDays);
            var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(request.LastMaintenanceActivityDate.ToDateTime(TimeOnly.MinValue), PreventiveSchedule.FrequencyInterval, frequencyUnit.Code ?? "", PreventiveSchedule.ReminderMaterialReqDays);

            var dto = new MapPreventiveScheduleDetailDto
            {
                PreventiveSchedulerHeaderId = PreventiveSchedule.Id,
                MachineId = request.MachineId,
                WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate),
                ActualWorkOrderDate = DateOnly.FromDateTime(nextDate),
                MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate),
                ScheduleId = PreventiveSchedule.ScheduleId,
                FrequencyTypeId = PreventiveSchedule.FrequencyTypeId,
                FrequencyInterval = PreventiveSchedule.FrequencyInterval,
                FrequencyUnitId = PreventiveSchedule.FrequencyUnitId,
                GraceDays = PreventiveSchedule.GraceDays,
                ReminderWorkOrderDays = PreventiveSchedule.ReminderWorkOrderDays,
                ReminderMaterialReqDays = PreventiveSchedule.ReminderMaterialReqDays,
                IsDownTimeRequired = PreventiveSchedule.IsDownTimeRequired,
                DownTimeEstimateHrs = PreventiveSchedule.DownTimeEstimateHrs,
                LastMaintenanceActivityDate = request.LastMaintenanceActivityDate
            };
            var preventiveScheduler = _mapper.Map<PreventiveSchedulerDetail>(dto);
            _logger.LogInformation("MapMachineCommandHandler PreventiveSchedulerDetailId:{PreventiveSchedulerDetailId},preventiveScheduler:{json}", PreventiveSchedule.Id, JsonConvert.SerializeObject(preventiveScheduler));
            var Preventiveresult = await _preventiveSchedulerCommand.CreateDetailAsync(preventiveScheduler);

            int jobDelayMin = 0;
            var targetDateTime = Preventiveresult.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
            var delay = targetDateTime - DateTime.Now;
            string newJobId;
            var delayInMinutes = (int)delay.TotalMinutes;
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            if (delay.TotalSeconds > 0)
            {
                newJobId = await _backgroundServiceClient.ScheduleWorkOrder(Preventiveresult.Id, delayInMinutes, token);
            }
            else
            {
                jobDelayMin += 2;
                newJobId = await _backgroundServiceClient.ScheduleWorkOrder(Preventiveresult.Id, jobDelayMin, token);
            }

            await _preventiveSchedulerCommand.UpdateDetailAsync(Preventiveresult.Id, newJobId);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "New machine mapped successfully"
            };
        }
    }
}
