// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IReports;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.Reports.WorkOrderItemConsuption
// {
//     public class WorkOrderIssueQueryHandler : IRequestHandler<WorkOrderIssueQuery, ApiResponseDTO<List<WorkOrderIssueDto>>>
//     {
//         private readonly IReportRepository _repository;
//         private readonly IMapper _mapper;
//          private readonly IMediator _mediator;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;

//         public WorkOrderIssueQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator, IUnitGrpcClient unitGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _repository = repository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _unitGrpcClient = unitGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<WorkOrderIssueDto>>> Handle(WorkOrderIssueQuery request, CancellationToken cancellationToken)
//         {
//             var fromDate = request.IssueFrom ?? throw new ArgumentNullException(nameof(request.IssueFrom));
//             var toDate = request.IssueTo ?? throw new ArgumentNullException(nameof(request.IssueTo));
//                    // Get data from repository
//             var workOrders = await _repository.GetItemConsumptionAsync(
//                         fromDate,
//                         toDate
                        
//                     );

//                     var workOrderDtos = _mapper.Map<List<WorkOrderIssueDto>>(workOrders);
//                      var units = await _unitGrpcClient.GetAllUnitAsync();
//                     var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
//                     var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//                     var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             foreach (var dto in workOrderDtos)
//             {
//                 if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
//                 {
//                     dto.UnitName = unitName;
//                 }

//                 if (departmentLookup.TryGetValue(dto.ProductionDepartmentId, out var departmentName))
//                 {
//                     dto.ProductionDepartmentName = departmentName;
//                 }
//                  if (departmentLookup.TryGetValue(dto.MaintenanceDepartmentId, out var maintenancedepartmentName))
//                 {
//                     dto.MaintenanceDepartmentName = maintenancedepartmentName;
//                 }

//             }

//                     // Domain event log
//             var domainEvent = new AuditLogsDomainEvent(
//                         actionDetail: "GetWorkOrderIssue",
//                         actionCode: "Get",
//                         actionName: workOrders.Count.ToString(),
//                         details: "Work order issue list fetched.",
//                         module: "WorkOrder"
//                     );
//                     await _mediator.Publish(domainEvent, cancellationToken);

//                     return new ApiResponseDTO<List<WorkOrderIssueDto>>
//                     {
//                         IsSuccess = true,
//                         Message = "Success",
//                         Data = workOrderDtos,
//                         TotalCount = workOrderDtos.Count
//                     };
//         }
//     }
// }