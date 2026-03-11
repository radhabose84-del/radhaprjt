using Contracts.Common;
using Contracts.Dtos.Maintenance.Preventive;
using Contracts.Events.Maintenance.PreventiveScheduler;
using MaintenanceManagement.Application.Common;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler
{
    public class CreatePreventiveSchedulerCommandHandler : IRequestHandler<CreatePreventiveSchedulerCommand, int>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        private readonly IMediator _mediator;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMaintenanceUnitOfWork _unitOfWork;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IMachineMasterQueryRepository _machineMasterQueryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly ILogger<CreatePreventiveSchedulerCommandHandler> _logger;

        public CreatePreventiveSchedulerCommandHandler(
            IPreventiveSchedulerCommand preventiveSchedulerCommand,
            IMediator mediator,
            IOutboxEventPublisher outboxEventPublisher,
            IMaintenanceUnitOfWork unitOfWork,
            IIPAddressService iPAddressService,
            IPreventiveScheduleLogService preventiveScheduleLogService,
            IMachineMasterQueryRepository machineMasterQueryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IPreventiveSchedulerQuery preventiveSchedulerQuery,
            ILogger<CreatePreventiveSchedulerCommandHandler> logger)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _mediator = mediator;
            _outboxEventPublisher = outboxEventPublisher;
            _unitOfWork = unitOfWork;
            _ipAddressService = iPAddressService;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _machineMasterQueryRepository = machineMasterQueryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _logger = logger;
        }

        public async Task<int> Handle(CreatePreventiveSchedulerCommand request, CancellationToken cancellationToken)
        {
            // ── Read-only work outside the transaction ─────────────────────────────
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var machineMaster = await _machineMasterQueryRepository.GetMachineByGroupSagaAsync(request.MachineGroupId, unitId);

            if (machineMaster == null || machineMaster.Count == 0)
                throw new ExceptionRules("No machines found for selected MachineGroup.");

            var frequencyUnit = await _miscMasterQueryRepository.GetByIdAsync(request.FrequencyUnitId);
            if (frequencyUnit == null || string.IsNullOrWhiteSpace(frequencyUnit.Code))
                throw new ExceptionRules("Invalid FrequencyUnitId.");

            var details = new List<PreventiveSchedulerDetail>();
            foreach (var machine in machineMaster)
            {
                var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(
                    request.EffectiveDate.ToDateTime(TimeOnly.MinValue),
                    request.FrequencyInterval,
                    frequencyUnit.Code,
                    request.ReminderWorkOrderDays);

                var (_, itemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(
                    request.EffectiveDate.ToDateTime(TimeOnly.MinValue),
                    request.FrequencyInterval,
                    frequencyUnit.Code,
                    request.ReminderMaterialReqDays);

                details.Add(new PreventiveSchedulerDetail
                {
                    PreventiveScheduler = null!,
                    Machine = null!,
                    MachineId = machine.Id,
                    WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate),
                    ActualWorkOrderDate = DateOnly.FromDateTime(nextDate),
                    MaterialReqStartDays = DateOnly.FromDateTime(itemReminderDate),
                    ScheduleId = request.ScheduleId,
                    FrequencyTypeId = request.FrequencyTypeId,
                    FrequencyInterval = request.FrequencyInterval,
                    FrequencyUnitId = request.FrequencyUnitId,
                    GraceDays = request.GraceDays,
                    ReminderWorkOrderDays = request.ReminderWorkOrderDays,
                    ReminderMaterialReqDays = request.ReminderMaterialReqDays,
                    IsDownTimeRequired = request.IsDownTimeRequired,
                    DownTimeEstimateHrs = request.DownTimeEstimateHrs,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }

            var preventiveScheduler = new PreventiveSchedulerHeader
            {
                MachineGroup = null!,
                MiscMaintenanceCategory = null!,
                MiscSchedule = null!,
                MiscFrequencyType = null!,
                MiscFrequencyUnit = null!,
                PreventiveSchedulerName = request.PreventiveSchedulerName,
                MachineGroupId = request.MachineGroupId,
                DepartmentId = request.DepartmentId,
                MaintenanceCategoryId = request.MaintenanceCategoryId,
                ScheduleId = request.ScheduleId,
                FrequencyTypeId = request.FrequencyTypeId,
                FrequencyInterval = request.FrequencyInterval,
                FrequencyUnitId = request.FrequencyUnitId,
                EffectiveDate = request.EffectiveDate,
                GraceDays = request.GraceDays,
                ReminderWorkOrderDays = request.ReminderWorkOrderDays,
                ReminderMaterialReqDays = request.ReminderMaterialReqDays,
                IsDownTimeRequired = request.IsDownTimeRequired,
                DownTimeEstimateHrs = request.DownTimeEstimateHrs,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                PreventiveSchedulerDetails = details,
                PreventiveSchedulerActivities = request.Activity?.Select(a => new PreventiveSchedulerActivity
                {
                    PreventiveScheduler = null!,
                    Activity = null!,
                    ActivityId = a.ActivityId
                }).ToList(),
                PreventiveSchedulerItems = request.Items?.Select(i => new PreventiveSchedulerItems
                {
                    PreventiveScheduler = null!,
                    OldItemId = i.ItemId,
                    RequiredQty = i.RequiredQty,
                    OldCategoryDescription = i.OldCategoryDescription,
                    OldGroupName = i.OldGroupName,
                    OldItemName = i.OldItemName
                }).ToList()
            };

            // ── Atomic transaction: domain insert + outbox insert ──────────────────
            // If any step throws, RollbackAsync undoes both the header/detail inserts
            // and the outbox row — nothing is left in an inconsistent state.
            // If CommitAsync succeeds, BSOFT.Worker's SqlOutboxProcessorJob picks up
            // the outbox row and publishes MachineWiseScheduleCreationEvent to RabbitMQ.
            PreventiveSchedulerHeader response;
            var correlationId = Guid.NewGuid();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                response = await _preventiveSchedulerCommand.CreateAsync(preventiveScheduler);
                if (response == null || response.Id <= 0)
                    throw new ExceptionRules("Preventive scheduler creation failed.");

                _logger.LogInformation("Created PreventiveSchedulerId: {Id}, Details: {Count}", response.Id, details.Count);

                var reverseMapDetails = (response.PreventiveSchedulerDetails ?? Enumerable.Empty<PreventiveSchedulerDetail>())
                    .Select(d =>
                    {
                        var target = d.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                        var minutes = (int)Math.Ceiling((target - DateTime.Now).TotalMinutes);
                        return new ScheduleDetailSagaDto
                        {
                            Id = d.Id,
                            DelayInMinutes = minutes < 0 ? 5 : minutes,
                            PreventiveSchedulerHeaderId = d.PreventiveSchedulerHeaderId,
                            MachineId = d.MachineId,
                            WorkOrderCreationStartDate = d.WorkOrderCreationStartDate,
                            ActualWorkOrderDate = d.ActualWorkOrderDate,
                            MaterialReqStartDays = d.MaterialReqStartDays,
                            FrequencyInterval = d.FrequencyInterval,
                            ReminderWorkOrderDays = d.ReminderWorkOrderDays,
                            ReminderMaterialReqDays = d.ReminderMaterialReqDays,
                            LastMaintenanceActivityDate = d.LastMaintenanceActivityDate,
                            ScheduleId = d.ScheduleId ?? 0,
                            FrequencyTypeId = d.FrequencyTypeId ?? 0,
                            FrequencyUnitId = d.FrequencyUnitId ?? 0,
                            DownTimeEstimateHrs = d.DownTimeEstimateHrs,
                            GraceDays = d.GraceDays,
                            IsDownTimeRequired = d.IsDownTimeRequired
                        };
                    })
                    .ToList();

                var @event = new MachineWiseScheduleCreationEvent
                {
                    CorrelationId = correlationId,
                    PreventiveSchedulerHeaderId = response.Id,
                    ScheduleDetail = reverseMapDetails
                };

                // ScheduleWithoutSaveAsync adds the outbox row to the EF change tracker
                // without calling SaveChangesAsync — CommitAsync flushes + commits both.
                await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            // ── Post-commit: audit + log (outside transaction — informational only) ─
            _logger.LogInformation("Committed MachineWiseScheduleCreationEvent for PreventiveSchedulerHeaderId: {Id}", response.Id);

            await AuditLogPublisher.PublishAuditLogAsync(
                _mediator,
                actionDetail: "Create",
                actionCode: "NewData",
                actionName: "Preventive Schedule Creation",
                module: "Preventive",
                requestData: request,
                cancellationToken);

            await _preventiveScheduleLogService.CaptureLogs(response.Id, null, "Create Header", JsonConvert.SerializeObject(request));
            return response.Id;
        }
    }
}
