using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Maintenance.Preventive;
using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler
{
    public class UpdatePreventiveSchedulerCommandHandler : IRequestHandler<UpdatePreventiveSchedulerCommand, ApiResponseDTO<bool>>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMaintenanceUnitOfWork _unitOfWork;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly ILogger<UpdatePreventiveSchedulerCommandHandler> _logger;

        public UpdatePreventiveSchedulerCommandHandler(
            IPreventiveSchedulerCommand preventiveSchedulerCommand,
            IMapper mapper,
            IMediator mediator,
            IPreventiveSchedulerQuery preventiveSchedulerQuery,
            IWorkOrderCommandRepository workOrderRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMaintenanceUnitOfWork unitOfWork,
            IOutboxEventPublisher outboxEventPublisher,
            IPreventiveScheduleLogService preventiveScheduleLogService,
            IHttpContextAccessor httpContextAccessor,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            ILogger<UpdatePreventiveSchedulerCommandHandler> logger)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _mapper = mapper;
            _mediator = mediator;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _workOrderRepository = workOrderRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _unitOfWork = unitOfWork;
            _outboxEventPublisher = outboxEventPublisher;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _httpContextAccessor = httpContextAccessor;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdatePreventiveSchedulerCommand request, CancellationToken cancellationToken)
        {
            await _preventiveScheduleLogService.CaptureLogs(request.Id, null, "Update", JsonConvert.SerializeObject(request));

            // ── Read-only work outside the transaction ─────────────────────────────
            var preventiveScheduler = _mapper.Map<PreventiveSchedulerHeader>(request);
            var existingPreventiveScheduler = await _preventiveSchedulerQuery.GetByIdAsync(request.Id);

            bool isFrequencyChanged =
                request.FrequencyInterval != existingPreventiveScheduler.FrequencyInterval ||
                request.FrequencyTypeId != existingPreventiveScheduler.FrequencyTypeId ||
                request.FrequencyUnitId != existingPreventiveScheduler.FrequencyUnitId;

            _logger.LogInformation(
                "Is Frequency Changed PreventiveSchedulerId:{PreventiveSchedulerId},isFrequencyChanged:{isFrequencyChanged}",
                request.Id, isFrequencyChanged);

            // ── Atomic transaction: metadata update + detail update + outbox insert ─
            PreventiveSchedulerHeader? metaDataResponse = null;
            var correlationId = Guid.NewGuid();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                metaDataResponse = await _preventiveSchedulerCommand.UpdateScheduleMetadata(preventiveScheduler);

                if (metaDataResponse != null && metaDataResponse.Id > 0)
                {
                    var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(metaDataResponse.FrequencyUnitId);
                    var DetailResult = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(metaDataResponse.Id);

                    var scheduleDetailUpdates = new List<ScheduleDetailUpdateDto>();

                    foreach (var detail in DetailResult)
                    {
                        if (isFrequencyChanged)
                        {
                            var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(
                                (detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue),
                                metaDataResponse.FrequencyInterval, frequencyUnit.Code ?? "", metaDataResponse.ReminderWorkOrderDays);

                            var (_, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(
                                (detail.LastMaintenanceActivityDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue),
                                metaDataResponse.FrequencyInterval, frequencyUnit.Code ?? "", metaDataResponse.ReminderMaterialReqDays);

                            detail.PreventiveSchedulerHeaderId = metaDataResponse.Id;
                            detail.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
                            detail.FrequencyInterval = metaDataResponse.FrequencyInterval;

                            var existsWO = await _preventiveSchedulerQuery.ExistWorkOrderBySchedulerDetailId(detail.Id);
                            _logger.LogInformation(
                                "Existing Work Order PreventiveSchedulerId:{PreventiveSchedulerId},IsExisting:{existsWO}",
                                request.Id, existsWO);

                            if (!existsWO)
                            {
                                detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate);
                                detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);

                                var delayMins = (int)Math.Ceiling((reminderDate - DateTime.Now).TotalMinutes);
                                scheduleDetailUpdates.Add(new ScheduleDetailUpdateDto
                                {
                                    Id = detail.Id,
                                    DelayInMinutes = delayMins < 5 ? 5 : delayMins
                                });
                            }
                        }
                        else
                        {
                            detail.ReminderWorkOrderDays = metaDataResponse.ReminderWorkOrderDays;
                            detail.ReminderMaterialReqDays = metaDataResponse.ReminderMaterialReqDays;
                        }
                    }

                    await _preventiveSchedulerCommand.UpdateScheduleDetails(metaDataResponse.Id, DetailResult);

                    if (scheduleDetailUpdates.Count > 0)
                    {
                        var @event = new HeaderUpdateEvent
                        {
                            CorrelationId = correlationId,
                            ScheduleDetailUpdate = scheduleDetailUpdates
                        };
                        await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, "maintenance", cancellationToken);

                        _logger.LogInformation(
                            "Queued HeaderUpdateEvent for PreventiveSchedulerHeaderId: {Id}, Details: {Count}",
                            metaDataResponse.Id, scheduleDetailUpdates.Count);
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            // ── Post-commit: audit ─────────────────────────────────────────────────
            await AuditLogPublisher.PublishAuditLogAsync(
                _mediator,
                actionDetail: "Schedule Update request",
                actionCode: "Schedule Update",
                actionName: "Schedule Update",
                module: "Preventive",
                requestData: request,
                cancellationToken);

            if (metaDataResponse?.Id > 0)
            {
                _logger.LogInformation("Updated PreventiveSchedulerId: {Id}", request.Id);
                return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Preventive Scheduler updated successfully." };
            }

            return new ApiResponseDTO<bool> { IsSuccess = false, Message = "Preventive Scheduler not updated." };
        }
    }
}
