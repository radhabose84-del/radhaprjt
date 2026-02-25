using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Reports.PowerConsumption
{
    public class PowerConsumptionReportQueryHandler  : IRequestHandler<PowerConsumptionReportQuery, ApiResponseDTO<List<PowerReportDto>>>
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup; // 👈 lookup dependency

        public PowerConsumptionReportQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<PowerReportDto>>> Handle(PowerConsumptionReportQuery request, CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? throw new ArgumentNullException(nameof(request.FromDate));
            var toDate = request.ToDate ?? throw new ArgumentNullException(nameof(request.ToDate));

            // Fetch AssetTransfer report data from repository
            var powerconsumptionReports = await _repository.GetPowerReports(fromDate, toDate);

            // Map to DTOs
            var powerconsumptionReportDtos = _mapper.Map<List<PowerReportDto>>(powerconsumptionReports);
            // 🔥 Fetch departments via lookup

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var powerconsumptionDictionary = new Dictionary<int, PowerReportDto>();

            // 🔥 Map department names to AssetTransferData
            foreach (var data in powerconsumptionReportDtos)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }
                powerconsumptionDictionary[data.DepartmentId] = data;

            }

            // Log audit
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "PowerConsumptionReportQuery",
                actionCode: "Get",
                actionName: powerconsumptionReportDtos.Count.ToString(),
                details: "PowerConsumption report list fetched.",
                module: "Power"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // Return API response
            return new ApiResponseDTO<List<PowerReportDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = powerconsumptionReportDtos ?? new List<PowerReportDto>(),
                TotalCount = powerconsumptionReportDtos != null ? powerconsumptionReportDtos.Count : 0
            };
        }
    }
}
