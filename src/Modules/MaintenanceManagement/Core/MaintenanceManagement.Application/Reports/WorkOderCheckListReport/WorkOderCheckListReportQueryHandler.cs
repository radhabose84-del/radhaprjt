using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOderCheckListReport
{
    public class WorkOderCheckListReportQueryHandler : IRequestHandler<WorkOderCheckListReportQuery, ApiResponseDTO<List<WorkOderCheckListReportDto>>>
    {

        private readonly IReportRepository _workOrderCheckListQueryRepository;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public WorkOderCheckListReportQueryHandler(
            IReportRepository workOrderCheckListQueryRepository,
            IMapper mapper,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _workOrderCheckListQueryRepository = workOrderCheckListQueryRepository;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ApiResponseDTO<List<WorkOderCheckListReportDto>>> Handle(WorkOderCheckListReportQuery request, CancellationToken cancellationToken)
        {

            var requestReportEntities = await _workOrderCheckListQueryRepository.GetWorkOrderChecklistReportAsync(
            request.WorkOrderFromDate,
            request.WorkOrderToDate,
            request.MachineGroupId,
            request.MachineId,
            request.ActivityId
           );

            var requestReportDtos = _mapper.Map<List<WorkOderCheckListReportDto>>(requestReportEntities);
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var dto in requestReportDtos)
            {
                if (companyLookup.TryGetValue(dto.CompanyId, out var companyName))
                {
                    dto.CompanyName = companyName;
                }

                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                {
                    dto.UnitName = unitName;
                }
            }
            // foreach (var dto in requestReportDtos)
            // {

            //     if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
            //     {
            //         dto.UnitName = unitName;
            //     }
            // }
            return new ApiResponseDTO<List<WorkOderCheckListReportDto>>
            {
                IsSuccess = requestReportDtos != null && requestReportDtos.Any(),
                Message = requestReportDtos != null && requestReportDtos.Any()
                    ? "WorkOderCheckList report retrieved successfully."
                    : "No WorkOderCheckList requests found.",
                Data = requestReportDtos
            };
        }


    }
}
