// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IReports;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.Reports.PowerConsumption
// {
//     public class PowerConsumptionReportQueryHandler  : IRequestHandler<PowerConsumptionReportQuery, ApiResponseDTO<List<PowerReportDto>>>
//     {
//         private readonly IReportRepository _repository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient; // 👈 gRPC Inject here

//         public PowerConsumptionReportQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _repository = repository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<PowerReportDto>>> Handle(PowerConsumptionReportQuery request, CancellationToken cancellationToken)
//         {
//             var fromDate = request.FromDate ?? throw new ArgumentNullException(nameof(request.FromDate));
//             var toDate = request.ToDate ?? throw new ArgumentNullException(nameof(request.ToDate));

//             // Fetch AssetTransfer report data from repository
//             var powerconsumptionReports = await _repository.GetPowerReports(fromDate, toDate);

//             // Map to DTOs
//             var powerconsumptionReportDtos = _mapper.Map<List<PowerReportDto>>(powerconsumptionReports);
//             // 🔥 Fetch departments using gRPC

//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var powerconsumptionDictionary = new Dictionary<int, PowerReportDto>();

//             // 🔥 Map department names to AssetTransferData
//             foreach (var data in powerconsumptionReportDtos)
//             {
//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.DepartmentName = departmentName;
//                 }
//                 powerconsumptionDictionary[data.DepartmentId] = data;

//             }

//             // Log audit
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "PowerConsumptionReportQuery",
//                 actionCode: "Get",
//                 actionName: powerconsumptionReportDtos.Count.ToString(),
//                 details: "PowerConsumption report list fetched.",
//                 module: "Power"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             // Return API response
//             return new ApiResponseDTO<List<PowerReportDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = powerconsumptionReportDtos ?? new List<PowerReportDto>(),
//                 TotalCount = powerconsumptionReportDtos != null ? powerconsumptionReportDtos.Count : 0
//             };
//         }
//     }
// }