using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId
{
    public class GetMachineDepartmentbyIdQueryHandler : IRequestHandler<GetMachineDepartmentbyIdQuery, MachineDepartmentGroupDto>
    {
        private readonly IMachineMasterQueryRepository _machineMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IDepartmentGroupLookup _departmentGroupLookup;

        public GetMachineDepartmentbyIdQueryHandler(
            IMachineMasterQueryRepository machineMasterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IDepartmentGroupLookup departmentGroupLookup)
        {
            _machineMasterQueryRepository = machineMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _departmentGroupLookup = departmentGroupLookup;
        }

        public async Task<MachineDepartmentGroupDto> Handle(GetMachineDepartmentbyIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _machineMasterQueryRepository.GetMachineDepartment(request.MachineGroupId);
            var machineMaster = _mapper.Map<MachineDepartmentGroupDto>(result);

            if (result != null && int.TryParse(result.DepartmentId, out var departmentId))
            {
                var department = await _departmentLookup.GetByIdAsync(departmentId);
                if (department != null)
                {
                    machineMaster.DepartmentName = department.DepartmentName;
                    machineMaster.DepartmentGroupId = department.Departmentgroupid;
                    if (department.Departmentgroupid > 0)
                    {
                        var group = await _departmentGroupLookup.GetByIdAsync(department.Departmentgroupid);
                        machineMaster.DepartmentGroupName = group?.DepartmentGroupName;
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMachineDepartmentbyIdQuery",
                actionName: machineMaster.DepartmentName ?? string.Empty,
                details: $"Machine department details {machineMaster.DepartmentGroupName} was fetched.",
                module: "MachineDepartment");

            await _mediator.Publish(domainEvent, cancellationToken);
            return machineMaster;
        }
    }
}
