using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MaintenanceRequestReport
{
    public class RequestReportQueryHandler : IRequestHandler<RequestReportQuery, ApiResponseDTO<List<RequestReportDto>>>
    {

        private readonly IReportRepository _maintenanceRequestQueryRepository;
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public RequestReportQueryHandler(IReportRepository maintenanceRequestQueryRepository, IMapper mapper, IDepartmentLookup departmentLookup, IUnitLookup unitLookup)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

        public async Task<ApiResponseDTO<List<RequestReportDto>>> Handle(RequestReportQuery request, CancellationToken cancellationToken)
        {

            var requestReportEntities = await _maintenanceRequestQueryRepository.MaintenanceReportAsync(
            request.RequestFromDate,
            request.RequestToDate,
            request.RequestType,
            request.RequestStatus,
            request.DepartmentId);


            var requestReportDtos = _mapper.Map<List<RequestReportDto>>(requestReportEntities);
               if (requestReportDtos == null || !requestReportDtos.Any())
                {
                    return new ApiResponseDTO<List<RequestReportDto>>
                    {
                        IsSuccess = false,
                        Message = "No maintenance requests found.", 
                        Data = new List<RequestReportDto>()
                    };
                }
                          
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();           
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            
             // 🔹 FILTER USING departmentLookup
            // Keep only those records whose DepartmentId exists in departmentLookup
            requestReportDtos = requestReportDtos
                .Where(dto => departmentLookup.ContainsKey(dto.MaintenanceDepartmentId))
                .ToList();

                  // If after filtering nothing left, return "no data"
            if (!requestReportDtos.Any())
            {
                return new ApiResponseDTO<List<RequestReportDto>>
                {
                    IsSuccess = false,
                    Message = "No maintenance requests found for valid departments.",
                    Data = new List<RequestReportDto>()
                };
            }
           
            foreach (var dto in requestReportDtos)
            {
                if (departmentLookup.TryGetValue(dto.MaintenanceDepartmentId, out var maintenanceDeptName))
                {
                    dto.MaintenanceDepartment = maintenanceDeptName;
                }

                if (departmentLookup.TryGetValue(dto.ProductionDepartmentId, out var productionDeptName))
                {
                    dto.ProductionDepartment = productionDeptName;
                }

                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                {
                    dto.UnitName = unitName;
                }
            }


           return new ApiResponseDTO<List<RequestReportDto>>
            {
                IsSuccess = true,
                Message = "Maintenance report retrieved successfully.",
                Data = requestReportDtos ?? new List<RequestReportDto>()
            };
        }




    }
}
