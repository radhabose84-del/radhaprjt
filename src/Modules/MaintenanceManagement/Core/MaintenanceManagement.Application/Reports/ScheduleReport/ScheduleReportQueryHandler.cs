// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IReports;
// using MediatR;

// namespace MaintenanceManagement.Application.Reports.ScheduleReport
// {
//     public class ScheduleReportQueryHandler : IRequestHandler<ScheduleReportQuery, ApiResponseDTO<List<ScheduleReportDto>>>
//     {
//         private readonly IReportRepository _reportQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IDepartmentGrpcClient _departmentGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IIPAddressService _ipAddressService;
//         public ScheduleReportQueryHandler(IReportRepository reportQueryRepository, IMapper mapper, IDepartmentGrpcClient departmentGrpcClient,
//         IDepartmentAllGrpcClient departmentAllGrpcClient, IUnitGrpcClient unitGrpcClient, IIPAddressService ipAddressService)
//         {
//             _reportQueryRepository = reportQueryRepository;
//             _mapper = mapper;
//             _departmentGrpcClient = departmentGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//             _ipAddressService = ipAddressService;
//         }
//         public async Task<ApiResponseDTO<List<ScheduleReportDto>>> Handle(ScheduleReportQuery request, CancellationToken cancellationToken)
//         {
//             var reportEntities = await _reportQueryRepository.ScheduleReportAsync(request.FromDueDate, request.ToDueDate) ?? new List<ScheduleReportDto>();

//             var preventiveSchedulerList = _mapper.Map<List<ScheduleReportDto>>(reportEntities) ?? new List<ScheduleReportDto>();

//             var productionDepartmentList = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var ProductiondepartmentLookup = productionDepartmentList.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//         //    var units = await _unitGrpcClient.GetUserUnitAsync(_ipAddressService.GetUserId());
//         //    var unitsLookup = units.ToDictionary(d => d.UnitId, d => d.UnitName);
//             // var PreventiveSchedulerDictionary = new Dictionary<int, ScheduleReportDto>();


//             // foreach (var data in preventiveSchedulerList)
//             // {

//             //     if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//             //     {
//             //         data.Department = departmentName;
//             //     }

//             //     PreventiveSchedulerDictionary[data.DepartmentId] = data;

//             // }

//             foreach (var dto in preventiveSchedulerList)
//             {
//                 if (departmentLookup.TryGetValue(dto.DepartmentId, out var departmentName))
//                 {
//                     dto.DepartmentName = departmentName;
//                 }
//                 if (ProductiondepartmentLookup.TryGetValue(dto.ProductionDepartmentId, out var ProductiondepartmentName))
//                 {
//                     dto.ProductionDepartmentName = ProductiondepartmentName;
//                 }
//             }

//                 var filteredPreventiveSchedulers = preventiveSchedulerList
//             .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
//             .ToList();



//             return new ApiResponseDTO<List<ScheduleReportDto>>
//             {
//                 IsSuccess = filteredPreventiveSchedulers.Any(),
//                 Message = filteredPreventiveSchedulers.Any()
//                  ? "Scheduler Report retrieved successfully."
//                  : "No Scheduler Report found.",
//                 Data = filteredPreventiveSchedulers
//             };
//         }
//     }
// }