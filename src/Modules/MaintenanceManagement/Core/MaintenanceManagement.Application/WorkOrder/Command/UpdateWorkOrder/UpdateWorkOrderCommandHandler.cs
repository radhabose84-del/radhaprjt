using AutoMapper;
using Contracts.Events.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, ApiResponseDTO<bool>>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IWorkOrderQueryRepository _workOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMaintenanceUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateWorkOrderCommandHandler> _logger;
        private readonly ILogQueryService _logQueryService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;

        public UpdateWorkOrderCommandHandler(
            IWorkOrderCommandRepository workOrderRepository,
            IMapper mapper,
            IWorkOrderQueryRepository workOrderQueryRepository,
            IMediator mediator,
            IOutboxEventPublisher outboxEventPublisher,
            IMaintenanceUnitOfWork unitOfWork,
            ILogger<UpdateWorkOrderCommandHandler> logger,
            ILogQueryService logQueryService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup,
            IHttpContextAccessor httpContextAccessor,
            ITimeZoneService timeZoneService,
            IPreventiveScheduleLogService preventiveScheduleLogService,
            IPreventiveSchedulerCommand preventiveSchedulerCommand)
        {
            _workOrderRepository = workOrderRepository;
            _mapper = mapper;
            _workOrderQueryRepository = workOrderQueryRepository;
            _mediator = mediator;
            _outboxEventPublisher = outboxEventPublisher;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _logQueryService = logQueryService ?? throw new ArgumentNullException(nameof(logQueryService));
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
            _httpContextAccessor = httpContextAccessor;
            _timeZoneService = timeZoneService;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            await _preventiveScheduleLogService.CaptureLogs(null, request.WorkOrder.PreventiveScheduleId, "Work Order Update", JsonConvert.SerializeObject(request));

            var updatedEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(request.WorkOrder);

            _logger.LogInformation("Work Order Status. updatedEntity: {@updatedEntity}", updatedEntity);

            // ── Atomic transaction: WO update + optional next-schedule creation + outbox insert ─
            // If any step throws, RollbackAsync undoes all writes.
            // If CommitAsync succeeds, BSOFT.Worker picks up the outbox row and publishes
            // NextSchedulerCreatedEvent to RabbitMQ for downstream Hangfire scheduling.
            bool updateResult;
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                updateResult = await _workOrderRepository.UpdateAsync(updatedEntity.Id, updatedEntity);

                if (updateResult)
                {
                    var miscMaster = await _workOrderRepository.GetMiscMasterByCodeAsync(MiscEnumEntity.MaintenanceStatusUpdate.Code);

                    if (updatedEntity.StatusId == miscMaster.Id && updatedEntity.PreventiveScheduleId.HasValue)
                    {
                        var nextPreventiveSchedule = await _preventiveSchedulerCommand.CreateNextSchedulerDetailAsync(updatedEntity.PreventiveScheduleId.Value);

                        _logger.LogInformation(
                            "Next preventive schedule created. NextPreventiveSchedule: {@NextPreventiveSchedule}",
                            nextPreventiveSchedule);

                        if (nextPreventiveSchedule != null)
                        {
                            var targetDateTime = nextPreventiveSchedule.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                            var delay = targetDateTime - DateTime.Now;
                            var delayInMinutes = (int)delay.TotalMinutes;
                            var correlationId = Guid.NewGuid();

                            var @event = new NextSchedulerCreatedEvent
                            {
                                CorrelationId = correlationId,
                                PreventiveSchedulerDetailId = nextPreventiveSchedule.Id,
                                DelayInMinutes = delayInMinutes,
                                WorkOrderId = updatedEntity.Id
                            };

                            await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);
                        }
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            // ── Post-commit: audit (outside transaction) ───────────────────────────
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: updatedEntity.WorkOrderDocNo ?? string.Empty,
                actionName: "WorkOrder Update",
                details: $"WorkOrder updated for ID {updatedEntity.Id}",
                module: "WorkOrder"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            if (updateResult)
            {
                // ── Post-commit: image rename (file I/O — outside transaction) ──────
                string? tempFilePath = request.WorkOrder.Image;
                if (tempFilePath != null)
                {
                    string baseDirectory = await _workOrderQueryRepository.GetBaseDirectoryAsync();
                    var units = await _unitLookup.GetAllUnitAsync();
                    var companies = await _companyLookup.GetAllCompanyAsync();
                    var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
                    var companyDict = companies.ToDictionary(u => u.CompanyId, u => u.CompanyName);

                    string? unitName = null;
                    string? companyName = null;

                    if (unitDict.TryGetValue(request.WorkOrder.UnitId, out var unitNameVal))
                        unitName = unitNameVal;
                    if (companyDict.TryGetValue(request.WorkOrder.CompanyId, out var companyNameVal))
                        companyName = companyNameVal;

                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName ?? string.Empty, unitName ?? string.Empty);
                    string filePath = Path.Combine(uploadPath, tempFilePath);
                    EnsureDirectoryExists(Path.GetDirectoryName(filePath));

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                        string newFileName = $"{request.WorkOrder.WorkOrderDocNo}{Path.GetExtension(tempFilePath)}";
                        string newFilePath = Path.Combine(directory, newFileName);

                        try
                        {
                            File.Move(filePath, newFilePath);
                            await _workOrderRepository.UpdateWOImageAsync(request.WorkOrder.Id, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to rename file: {ErrorMessage}", ex.Message);
                        }
                    }
                }

                return new ApiResponseDTO<bool> { IsSuccess = true, Message = "WorkOrder updated." };
            }

            return new ApiResponseDTO<bool> { IsSuccess = false, Message = "WorkOrder not updated." };
        }

        private static void EnsureDirectoryExists(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
