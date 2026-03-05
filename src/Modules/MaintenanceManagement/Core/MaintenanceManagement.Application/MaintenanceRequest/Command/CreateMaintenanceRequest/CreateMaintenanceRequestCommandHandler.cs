#nullable disable
using AutoMapper;
using Contracts.Events.Notifications;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces.Lookups.Users;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest
{
    /// <summary>
    /// Handles creation of maintenance requests with transactional outbox pattern.
    ///
    /// Flow:
    /// ┌─────────────────────────────────────────────────────────────────────┐
    /// │  BEGIN TRANSACTION                                                   │
    /// │    1. Create MaintenanceRequest                                      │
    /// │    2. Create WorkOrder (if internal type)                           │
    /// │    3. Save NotificationCreatedEvent to OutboxMessages table         │
    /// │  COMMIT TRANSACTION                                                  │
    /// │                                                                      │
    /// │  ✅ Return 200 OK to user immediately                               │
    /// └─────────────────────────────────────────────────────────────────────┘
    ///
    /// Background (async):
    /// ┌─────────────────────────────────────────────────────────────────────┐
    /// │  OutboxPublisherBackgroundService (every 5 seconds)                 │
    /// │    1. Poll OutboxMessages WHERE Status = Pending                    │
    /// │    2. Publish NotificationCreatedEvent to RabbitMQ                  │
    /// │    3. Mark OutboxMessage as Published                               │
    /// │                                                                      │
    /// │  NotificationDispatcherConsumer                                      │
    /// │    1. Resolve channels (Email, SMS, WhatsApp, InApp)                │
    /// │    2. Dispatch to channel-specific consumers                        │
    /// └─────────────────────────────────────────────────────────────────────┘
    /// </summary>
    public class CreateMaintenanceRequestCommandHandler
        : IRequestHandler<CreateMaintenanceRequestCommand, ApiResponseDTO<int>>
    {
        private readonly IMaintenanceRequestCommandRepository _maintenanceRequestCommandRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly IWorkOrderCommandRepository _workOrderCommandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ILogger<CreateMaintenanceRequestCommandHandler> _logger;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDepartmentLookup _departmentLookup;

        public CreateMaintenanceRequestCommandHandler(
            IMaintenanceRequestCommandRepository maintenanceRequestCommandRepository,
            IMapper imapper,
            IMediator mediator,
            IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
            IWorkOrderCommandRepository workOrderCommandRepository,
            IIPAddressService ipAddressService,
            ILogger<CreateMaintenanceRequestCommandHandler> logger,
            IOutboxEventPublisher outboxEventPublisher,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDepartmentLookup departmentLookup)
        {
            _maintenanceRequestCommandRepository = maintenanceRequestCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _workOrderCommandRepository = workOrderCommandRepository;
            _ipAddressService = ipAddressService;
            _logger = logger;
            _outboxEventPublisher = outboxEventPublisher;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateMaintenanceRequestCommand request,
            CancellationToken cancellationToken)
        {
            // Generate correlation ID for distributed tracing
            var correlationId = Guid.NewGuid();

            _logger.LogInformation(
                "Creating Maintenance Request. CorrelationId: {CorrelationId}, MachineId: {MachineId}",
                correlationId, request.MachineId);

            // Get Open status for maintenance requests
            var statuses = await _maintenanceRequestQueryRepository.GetMaintenanceOpenstatusAsync();
            var openStatus = statuses.FirstOrDefault();

            if (openStatus == null)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Open status not configured for maintenance requests."
                };
            }

            // Map request to domain entity
            var maintenanceRequest = _imapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(request);

            // Set status and company/unit from context
            maintenanceRequest.RequestStatusId = openStatus.Id;
            maintenanceRequest.CompanyId = _ipAddressService.GetCompanyId();
            maintenanceRequest.UnitId = _ipAddressService.GetUnitId();

            // ═══════════════════════════════════════════════════════════════════
            // ATOMIC PATTERN:
            //   Step 1: CreateAsync(MaintenanceRequest) — saved immediately (Id needed for FK)
            //   Step 2: CreateWithoutSaveAsync(WorkOrder)   — tracked only
            //   Step 3: ScheduleWithoutSaveAsync(Outbox)    — tracked only
            //   CommitAsync() — one SaveChangesAsync atomically commits WorkOrder + OutboxMessage
            // ═══════════════════════════════════════════════════════════════════

            // Step 1: Save MaintenanceRequest to obtain its generated Id (required as FK for WorkOrder)
            var result = await _maintenanceRequestCommandRepository.CreateAsync(maintenanceRequest);
            if (result <= 0)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Failed to create Maintenance Request"
                };
            }

            // Get request type info
            var requestTypes = await _maintenanceRequestQueryRepository.GetMaintenanceRequestTypeAsync();
            var internalTypeId = requestTypes.FirstOrDefault()?.Id;

            // Get machine info
            var machineInfo = await _maintenanceRequestQueryRepository
                .GetMachineInfoAsync(maintenanceRequest.MachineId);
            if (machineInfo is null)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = $"Machine with Id {maintenanceRequest.MachineId} not found."
                };
            }
            var machineName = machineInfo.Value.MachineName;
            var departmentId = machineInfo.Value.DepartmentId;

            // Step 2: Track WorkOrder (only for internal type, no save yet)
            if (internalTypeId.HasValue && maintenanceRequest.RequestTypeId == internalTypeId.Value)
            {
                var workOrder = _imapper.Map<Domain.Entities.WorkOrderMaster.WorkOrder>(maintenanceRequest);
                workOrder.RequestId = maintenanceRequest.Id; // Real Id — MaintenanceRequest was saved in Step 1
                workOrder.CompanyId = _ipAddressService.GetCompanyId();
                workOrder.UnitId = _ipAddressService.GetUnitId();
                workOrder.Remarks = string.Empty;

                await _workOrderCommandRepository.CreateWithoutSaveAsync(
                    workOrder, request.MaintenanceTypeId, cancellationToken);

                // Get workflow event type
                var wfCreate = await _miscMasterQueryRepository.GetByWFMiscMasterCodeAsync(
                    MiscEnumEntity.WorkFlowCreate);

                // Get breakdown maintenance type
                var breakDownCode = await _miscMasterQueryRepository.GetByMiscMasterCodeAsync(
                    MiscEnumEntity.BreakDown);

                // Resolve department names
                var departments = await _departmentLookup.GetAllDepartmentAsync();
                var departmentDict = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                string productionDeptName = departmentDict.TryGetValue(request.ProductionDepartmentId, out var prodName)
                    ? prodName ?? string.Empty
                    : string.Empty;

                string maintenanceDeptName = departmentDict.TryGetValue(request.MaintenanceDepartmentId, out var maintName)
                    ? maintName ?? string.Empty
                    : string.Empty;

                var createdDate = workOrder.CreatedDate ?? DateTimeOffset.UtcNow;
                var createdBy = workOrder.CreatedByName ?? string.Empty;

                // Step 3: Track OutboxMessage (no save yet — participates in same transaction)
                var notificationEvent = CreateNotificationEvent(
                    correlationId: correlationId,
                    workOrder: workOrder,
                    isBreakdown: request.MaintenanceTypeId == breakDownCode.Id,
                    eventTypeId: wfCreate.Id,
                    departmentId: departmentId,
                    productionDeptName: productionDeptName,
                    maintenanceDeptName: maintenanceDeptName,
                    machineName: machineName,
                    remarks: request.Remarks ?? string.Empty,
                    createdDate: createdDate,
                    createdBy: createdBy);

                await _outboxEventPublisher.ScheduleWithoutSaveAsync(notificationEvent, correlationId, cancellationToken);
            }

            // ═══════════════════════════════════════════════════════════════════
            // COMMIT — single SaveChangesAsync atomically persists:
            //   WorkOrder + OutboxMessage (MaintenanceRequest already saved in Step 1)
            // ═══════════════════════════════════════════════════════════════════
            await _maintenanceRequestCommandRepository.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Outbox event tracked. CorrelationId: {CorrelationId}, MaintenanceRequestId: {Id}",
                correlationId, result);

            // Publish domain event for auditing (in-process, no message broker)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: request.MachineId.ToString(),
                actionName: "Maintenance Request Created",
                details: $"Maintenance Request created. CorrelationId: {correlationId}",
                module: "MaintenanceRequest"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation(
                "✅ Maintenance Request created successfully. Id: {Id}, CorrelationId: {CorrelationId}",
                result, correlationId);

            // Return immediately - notification will be sent asynchronously by background service
            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Maintenance Request created successfully",
                Data = result
            };
        }

        /// <summary>
        /// Creates the notification event with all parameters for template rendering
        /// </summary>
        private NotificationCreatedEvent CreateNotificationEvent(
            Guid correlationId,
            MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder,
            bool isBreakdown,
            int eventTypeId,
            int departmentId,
            string productionDeptName,
            string maintenanceDeptName,
            string machineName,
            string remarks,
            DateTimeOffset createdDate,
            string createdBy)
        {
            return new NotificationCreatedEvent
            {
                CorrelationId = correlationId,
                CreatedByName = workOrder.CreatedByName,
                UnitId = _ipAddressService.GetUnitId(),
                ModuleName = isBreakdown ? "WorkOrder-BreakDown" : "WorkOrder",
                EventTypeId = eventTypeId,
                // CC mail uses department ID for recipient resolution
                ccMail = isBreakdown ? departmentId.ToString() : string.Empty,

                // Template parameters (used by notification template engine)
                param1 = workOrder.Id.ToString(),
                param2 = isBreakdown ? productionDeptName : machineName,
                param3 = createdDate,
                param4 = isBreakdown ? machineName : productionDeptName,
                param5 = maintenanceDeptName,
                param6 = remarks,
                param7 = isBreakdown ? createdBy : string.Empty,

                // Additional parameters available for templates
                param8 = string.Empty,
                param9 = string.Empty,
                param10 = string.Empty
            };
        }
    }
}
