using AutoMapper;
using Contracts.Dtos.Maintenance.Preventive;
using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
using MaintenanceManagement.Application.Common;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
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
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMaintenanceUnitOfWork _unitOfWork;
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
            IOutboxEventPublisher outboxEventPublisher,
            IMaintenanceUnitOfWork unitOfWork,
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
            _outboxEventPublisher = outboxEventPublisher;
            _unitOfWork = unitOfWork;
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

            var rollbackHeader = _mapper.Map<RollbackHeaderDto>(existingPreventiveScheduler);

            bool isFrequencyChanged =
                request.FrequencyInterval != existingPreventiveScheduler.FrequencyInterval ||
                request.FrequencyTypeId != existingPreventiveScheduler.FrequencyTypeId ||
                request.FrequencyUnitId != existingPreventiveScheduler.FrequencyUnitId;

            _logger.LogInformation(
                "Is Frequency Changed PreventiveSchedulerId:{PreventiveSchedulerId},isFrequencyChanged:{isFrequencyChanged}",
                request.Id, isFrequencyChanged);

            // ── Atomic transaction: metadata update + detail update + outbox insert ─
            // If any step throws, RollbackAsync undoes all writes (header, details, outbox).
            // If CommitAsync succeeds, BSOFT.Worker picks up the outbox row and publishes
            // HeaderUpdateEvent to RabbitMQ for downstream Hangfire rescheduling.
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

                    var rollbackDetails = new List<RollbackScheduleDetailDto>();
                    var hangfireScheduleDetail = new List<ScheduleDetailUpdateDto>();

                    foreach (var detail in DetailResult)
                    {
                        var dto = _mapper.Map<RollbackScheduleDetailDto>(detail);

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

                            var result = await _preventiveSchedulerQuery.ExistWorkOrderBySchedulerDetailId(detail.Id);
                            _logger.LogInformation(
                                "Existing Work Order PreventiveSchedulerId:{PreventiveSchedulerId},IsExisting:{result}",
                                request.Id, result);

                            if (result != true)
                            {
                                rollbackDetails.Add(dto);
                                detail.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate);
                                detail.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);
                                hangfireScheduleDetail.Add(_mapper.Map<ScheduleDetailUpdateDto>(detail));
                            }
                        }
                        else
                        {
                            rollbackDetails.Add(dto);
                            detail.ReminderWorkOrderDays = metaDataResponse.ReminderWorkOrderDays;
                            detail.ReminderMaterialReqDays = metaDataResponse.ReminderMaterialReqDays;
                        }
                    }

                    await _preventiveSchedulerCommand.UpdateScheduleDetails(metaDataResponse.Id, DetailResult);

                    var @event = new HeaderUpdateEvent
                    {
                        CorrelationId = correlationId,
                        ScheduleDetailUpdate = hangfireScheduleDetail,
                        rollbackHeaders = rollbackHeader,
                        rollbackDetails = rollbackDetails
                    };

                    // Outbox insert participates in the same SQL transaction.
                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            // ── Post-commit: audit (outside transaction) ───────────────────────────
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
                _logger.LogInformation("Committed HeaderUpdateEvent for PreventiveSchedulerId: {Id}", request.Id);
                return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Preventive Scheduler updated successfully." };
            }

            return new ApiResponseDTO<bool> { IsSuccess = false, Message = "Preventive Scheduler not updated." };
        }
    }
}
