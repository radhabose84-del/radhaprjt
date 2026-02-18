using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup
{
    public class GetMachineGroupQueryHandler : IRequestHandler<GetMachineGroupQuery, ApiResponseDTO<List<MachineGroupDto>>>
    {
        private readonly IMachineGroupQueryRepository _machineGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;


        public GetMachineGroupQueryHandler(
            IMachineGroupQueryRepository machineGroupQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup,
            IIPAddressService iPAddressService)
        {
            _machineGroupQueryRepository = machineGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _ipAddressService = iPAddressService;
        }

        public async Task<ApiResponseDTO<List<MachineGroupDto>>> Handle(GetMachineGroupQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId();
            // Fetch data from repository
            var (machineGroups, totalCount) = await _machineGroupQueryRepository.GetAllMachineGroupsAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // Map domain entities to DTOs
            var machineGroupList = _mapper.Map<List<MachineGroupDto>>(machineGroups);

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var dto in machineGroupList)
            {
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;
            }
            // Publish domain event for auditing
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "MachineGroup details were fetched.",
                module: "MachineGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // Return API response
            return new ApiResponseDTO<List<MachineGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = machineGroupList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
