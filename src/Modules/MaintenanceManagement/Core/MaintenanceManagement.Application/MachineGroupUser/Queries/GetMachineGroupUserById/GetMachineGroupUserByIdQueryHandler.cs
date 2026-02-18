
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserById
{
    public class GetMachineGroupUserByIdQueryHandler : IRequestHandler<GetMachineGroupUserByIdQuery, MachineGroupUserDto>
    {
        private readonly IMachineGroupUserQueryRepository _machineGroupQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetMachineGroupUserByIdQueryHandler(IMachineGroupUserQueryRepository machineGroupQuery, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _machineGroupQuery = machineGroupQuery;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }
        public async Task<MachineGroupUserDto> Handle(GetMachineGroupUserByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _machineGroupQuery.GetByIdAsync(request.Id);
            var machineGroupResult = _mapper.Map<MachineGroupUserDto>(result);
           
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            // 🔥 Map department name
            if (departmentLookup.TryGetValue(machineGroupResult.DepartmentId, out var departmentName) && departmentName != null)
            {
                machineGroupResult.DepartmentName = departmentName;
            }
            
          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"MachineGroup User details was fetched.",
                    module:"MachineGroup User "
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return machineGroupResult;
        }
    }
}