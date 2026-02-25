#nullable disable
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MediatR;

namespace MaintenanceManagement.Application.Reports.ScheduleReport
{
    public class ScheduleReportQueryHandler : IRequestHandler<ScheduleReportQuery, ApiResponseDTO<List<ScheduleReportDto>>>
    {
        private readonly IReportRepository _reportQueryRepository;
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;
        public ScheduleReportQueryHandler(IReportRepository reportQueryRepository, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _reportQueryRepository = reportQueryRepository;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<List<ScheduleReportDto>>> Handle(ScheduleReportQuery request, CancellationToken cancellationToken)
        {
            var reportEntities = await _reportQueryRepository.ScheduleReportAsync(request.FromDueDate, request.ToDueDate) ?? new List<ScheduleReportDto>();

            var preventiveSchedulerList = _mapper.Map<List<ScheduleReportDto>>(reportEntities) ?? new List<ScheduleReportDto>();

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var ProductiondepartmentLookup = departmentLookup;

            foreach (var dto in preventiveSchedulerList)
            {
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var departmentName))
                {
                    dto.DepartmentName = departmentName;
                }
                if (ProductiondepartmentLookup.TryGetValue(dto.ProductionDepartmentId, out var ProductiondepartmentName))
                {
                    dto.ProductionDepartmentName = ProductiondepartmentName;
                }
            }

                var filteredPreventiveSchedulers = preventiveSchedulerList
            .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            .ToList();



            return new ApiResponseDTO<List<ScheduleReportDto>>
            {
                IsSuccess = filteredPreventiveSchedulers.Any(),
                Message = filteredPreventiveSchedulers.Any()
                 ? "Scheduler Report retrieved successfully."
                 : "No Scheduler Report found.",
                Data = filteredPreventiveSchedulers
            };
        }
    }
}
