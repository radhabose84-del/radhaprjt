#nullable disable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MaterialPlanningReport
{
    public class MaterialPlanningReportQueryHandler : IRequestHandler<MaterialPlanningReportQuery, ApiResponseDTO<List<MaterialPlanningReportDto>>>
    {
        private readonly IReportRepository _reportQueryRepository;
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;
        public MaterialPlanningReportQueryHandler(IReportRepository reportQueryRepository, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _reportQueryRepository = reportQueryRepository;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<List<MaterialPlanningReportDto>>> Handle(MaterialPlanningReportQuery request, CancellationToken cancellationToken)
        {
            var reportEntities = await _reportQueryRepository.MaterialPlanningReportAsync(request.FromDueDate, request.ToDueDate)  ?? new List<MaterialPlanningReportDto>();

            var MaterialPlanningList = _mapper.Map<List<MaterialPlanningReportDto>>(reportEntities) ?? new List<MaterialPlanningReportDto>();
            
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            
             foreach (var dto in MaterialPlanningList)
        {
            if (departmentLookup.TryGetValue(dto.ProductionDepartmentId, out var departmentName))
            {
                dto.ProductionDepartment = departmentName;
            }
        }

                var filteredMaterialPlanningList = MaterialPlanningList
            .Where(p => departmentLookup.ContainsKey(p.ProductionDepartmentId))
            .ToList();
            
               return new ApiResponseDTO<List<MaterialPlanningReportDto>>
            {
                IsSuccess = filteredMaterialPlanningList != null && filteredMaterialPlanningList.Any(),
                Message = filteredMaterialPlanningList != null && filteredMaterialPlanningList.Any()
         ? "Material Planning Report retrieved successfully."
         : "No Material Planning Report found.",
                Data = filteredMaterialPlanningList ?? new List<MaterialPlanningReportDto>()
            };
        }
    }
}
