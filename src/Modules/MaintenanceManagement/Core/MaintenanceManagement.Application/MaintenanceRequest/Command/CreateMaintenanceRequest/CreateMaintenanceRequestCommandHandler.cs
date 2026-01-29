// using AutoMapper;
// using Contracts.Events.Notifications;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
// using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Domain.Common;
// using MaintenanceManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;
// using Contracts.Interfaces.External.IUser; 

// namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest
// {
//     public class CreateMaintenanceRequestCommandHandler 
//         : IRequestHandler<CreateMaintenanceRequestCommand, ApiResponseDTO<int>>
//     {
//         private readonly IMaintenanceRequestCommandRepository _maintenanceRequestCommandRepository;
//         private readonly IMapper _imapper;
//         private readonly IMediator _mediator;
//         private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
//         private readonly IWorkOrderCommandRepository _workOrderCommandRepository;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly ILogger<CreateMaintenanceRequestCommandHandler> _logger;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IDepartmentAllGrpcClient _departmentGrpcClient;  

//         public CreateMaintenanceRequestCommandHandler(
//             IMaintenanceRequestCommandRepository maintenanceRequestCommandRepository,
//             IMapper imapper,
//             IMediator mediator,
//             IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
//             IWorkOrderCommandRepository workOrderCommandRepository,
//             IIPAddressService ipAddressService,
//             ILogger<CreateMaintenanceRequestCommandHandler> logger,
//             IEventPublisher eventPublisher,
//             IMiscMasterQueryRepository miscMasterQueryRepository,
//             IDepartmentAllGrpcClient departmentGrpcClient)   
//         {
//             _maintenanceRequestCommandRepository = maintenanceRequestCommandRepository;
//             _imapper = imapper;
//             _mediator = mediator;
//             _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
//             _workOrderCommandRepository = workOrderCommandRepository;
//             _ipAddressService = ipAddressService;
//             _logger = logger;
//             _eventPublisher = eventPublisher;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _departmentGrpcClient = departmentGrpcClient;   
//         }

//         public async Task<ApiResponseDTO<int>> Handle(
//             CreateMaintenanceRequestCommand request,
//             CancellationToken cancellationToken)
//         {
//             // 🔹 Misc status (Open)
//             var statuses   = await _maintenanceRequestQueryRepository.GetMaintenanceOpenstatusAsync();
//             var openStatus = statuses.FirstOrDefault();

//             if (openStatus == null)
//             {
//                 return new ApiResponseDTO<int>
//                 {
//                     IsSuccess = false,
//                     Message   = "Open status not configured for maintenance requests."
//                 };
//             }

//             // 🔹 Map request to domain entity
//             var maintenanceRequest = _imapper.Map<Core.Domain.Entities.MaintenanceRequest>(request);

//             // 🔹 Override status and company/unit from IP address service
//             maintenanceRequest.RequestStatusId = openStatus.Id;
//             maintenanceRequest.CompanyId       = _ipAddressService.GetCompanyId();
//             maintenanceRequest.UnitId          = _ipAddressService.GetUnitId();

//             // 🔹 Insert into the database
//             var result = await _maintenanceRequestCommandRepository.CreateAsync(maintenanceRequest);
//             if (result <= 0)
//             {
//                 return new ApiResponseDTO<int>
//                 {
//                     IsSuccess = false,
//                     Message   = "Failed to create Maintenance Request"
//                 };
//             }

//             // 🔹 Get internal request type
//             var requestTypes   = await _maintenanceRequestQueryRepository.GetMaintenanceRequestTypeAsync();
//             var internalTypeId = requestTypes.FirstOrDefault()?.Id;

//             // 🔹 Get machine name
//             var machineInfo  = await _maintenanceRequestQueryRepository
//                 .GetMachineInfoAsync(maintenanceRequest.MachineId);
//             if (machineInfo is null)
//             {
//                 return new ApiResponseDTO<int>
//                 {
//                     IsSuccess = false,
//                     Message   = $"Machine with Id {maintenanceRequest.MachineId} not found."
//                 };
//             }
//             var machineName  = machineInfo.Value.MachineName;
//             var departmentId = machineInfo.Value.DepartmentId;

//             // 🔹 Create WorkOrder only for internal type
//             if (internalTypeId.HasValue && maintenanceRequest.RequestTypeId == internalTypeId.Value)
//             {
//                 var workOrder = _imapper.Map<Core.Domain.Entities.WorkOrderMaster.WorkOrder>(maintenanceRequest);
//                 workOrder.RequestId = result;
//                 workOrder.CompanyId = _ipAddressService.GetCompanyId();
//                 workOrder.UnitId = _ipAddressService.GetUnitId();
//                 workOrder.Remarks = string.Empty;

//                 await _workOrderCommandRepository.CreateAsync(
//                     workOrder, request.MaintenanceTypeId, cancellationToken);

//                 // 🔹 Get Workflow Create EventType (Misc)
//                 var wfCreate = await _miscMasterQueryRepository.GetByWFMiscMasterCodeAsync(
//                     MiscEnumEntity.WorkFlowCreate);

//                 // 🔹 Get Breakdown MaintenanceType code
//                 var breakDownCode = await _miscMasterQueryRepository.GetByMiscMasterCodeAsync(
//                     MiscEnumEntity.BreakDown);

//                 // 🔹 Fetch departments using gRPC once
//                 var departments = await _departmentGrpcClient.GetDepartmentAllAsync();
//                 var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//                 string productionDeptName = string.Empty;
//                 string maintenanceDeptName = string.Empty;

//                 if (departmentLookup.TryGetValue(request.ProductionDepartmentId, out var prodName)
//                     && !string.IsNullOrWhiteSpace(prodName))
//                 {
//                     productionDeptName = prodName;
//                 }

//                 if (departmentLookup.TryGetValue(request.MaintenanceDepartmentId, out var maintName)
//                     && !string.IsNullOrWhiteSpace(maintName))
//                 {
//                     maintenanceDeptName = maintName;
//                 }
                
//                 var correlationId = Guid.NewGuid();
//                 var createdDate = workOrder.CreatedDate ?? DateTimeOffset.UtcNow;
//                 var createdBy = workOrder.CreatedByName;

//                 if (request.MaintenanceTypeId == breakDownCode.Id)
//                 {
//                     var @event = new NotificationCreatedEvent
//                     {
//                         CorrelationId = correlationId,
//                         CreatedByName = workOrder.CreatedByName,
//                         UnitId = _ipAddressService.GetUnitId(),
//                         ModuleName = "WorkOrder-BreakDown",
//                         EventTypeId = wfCreate.Id,
//                         ccMail = departmentId.ToString(),
//                         param1 = workOrder.Id.ToString(),
//                         param2 = productionDeptName,
//                         param3 = createdDate,
//                         param4 = machineName,
//                         param5 = maintenanceDeptName, 
//                         param6 = request.Remarks ?? string.Empty,
//                         param7 = createdBy ?? string.Empty
//                     };

//                     await _eventPublisher.SaveEventAsync(@event);
//                     await _eventPublisher.PublishPendingEventsAsync();

//                     _logger.LogInformation(
//                         "✅ Breakdown Workorder Created. CorrId={CorrelationId}, WorkOrderId={WorkOrderId}, ProdDept={ProdDept}, MaintDept={MaintDept}",
//                         correlationId, workOrder.Id, productionDeptName, maintenanceDeptName);
//                 }
//                 else
//                 {
//                     // 🔔 Non-breakdown WorkOrder notification
//                     var @event = new NotificationCreatedEvent
//                     {
//                         CorrelationId = correlationId,
//                         CreatedByName = workOrder.CreatedByName,
//                         UnitId = _ipAddressService.GetUnitId(),
//                         ModuleName = "WorkOrder",
//                         EventTypeId = wfCreate.Id,

//                         param1 = workOrder.Id.ToString(),
//                         param2 = machineName,
//                         param3 = createdDate,
//                         param4 = productionDeptName,
//                         param5 = maintenanceDeptName,
//                         param6 = request.Remarks ?? string.Empty
//                     };

//                     await _eventPublisher.SaveEventAsync(@event);
//                     await _eventPublisher.PublishPendingEventsAsync();

//                     _logger.LogInformation(
//                         "✅ Maintenance Request Workorder Created. CorrId={CorrelationId}, WorkOrderId={WorkOrderId}, ProdDept={ProdDept}, MaintDept={MaintDept}",
//                         correlationId, workOrder.Id, productionDeptName, maintenanceDeptName);
//                 }
//             }

//             // 🔹 Publish domain event for auditing/logging
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: request.MachineId.ToString(),
//                 actionName: "Maintenance Request Created",
//                 details: "Maintenance Request was created.",
//                 module: "MaintenanceRequest"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             // 🔹 Return success response
//             return new ApiResponseDTO<int>
//             {
//                 IsSuccess = true,
//                 Message   = "Maintenance Request created successfully",
//                 Data      = result
//             };
//         }
//     }
// }
