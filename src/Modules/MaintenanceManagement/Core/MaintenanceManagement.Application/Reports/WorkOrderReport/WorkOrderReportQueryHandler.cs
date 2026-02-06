using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOrderReport
{
    public class WorkOrderReportQueryHandler  : IRequestHandler<WorkOrderReportQuery, ApiResponseDTO<List<WorkOrderReportDto>>>
    {
        private readonly IReportRepository _reportQueryRepository;
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;

        public WorkOrderReportQueryHandler(IReportRepository reportQueryRepository, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _reportQueryRepository = reportQueryRepository;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<WorkOrderReportDto>>> Handle(WorkOrderReportQuery request, CancellationToken cancellationToken)
        {
            var reportEntities = await _reportQueryRepository.WorkOrderReportAsync(request.FromDate, request.ToDate, request.RequestTypeId, request.DepartmentId)
                                 ?? new List<WorkOrderReportDto>();
            var reportDto = _mapper.Map<List<WorkOrderReportDto>>(reportEntities) ?? new List<WorkOrderReportDto>();

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var filteredWorkOrders = reportDto
                .Where(p => departmentLookup.ContainsKey(p.DepartmentId) || departmentLookup.ContainsKey(p.ProductionDepartmentId))
                .Select(p =>
                {
                    if (departmentLookup.TryGetValue(p.DepartmentId, out var deptName))
                        p.Department = deptName;

                    if (departmentLookup.TryGetValue(p.ProductionDepartmentId, out var prodDeptName))
                        p.ProductionDepartment = prodDeptName;

                    return p;
                })
                .ToList();

            return new ApiResponseDTO<List<WorkOrderReportDto>>
            {
                IsSuccess = reportDto.Any(),
                Message = reportDto.Any()
                    ? "Work Order Report retrieved successfully."
                    : "No Work Order Report found.",
                Data = filteredWorkOrders
            };
        }
    }
}
