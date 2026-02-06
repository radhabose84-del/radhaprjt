using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
{
    public class GetMachineGroupUserAutoCompleteQueryHandler
        : IRequestHandler<GetMachineGroupUserAutoCompleteQuery, List<MachineGroupUserAutoCompleteDto>>
    {
        private readonly IMachineGroupUserQueryRepository _machineGroupQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetMachineGroupUserAutoCompleteQueryHandler(
            IMachineGroupUserQueryRepository machineGroupQuery,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup)
        {
            _machineGroupQuery = machineGroupQuery;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<List<MachineGroupUserAutoCompleteDto>> Handle(
            GetMachineGroupUserAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _machineGroupQuery.GetMachineGroupUserByName(request.SearchPattern ?? string.Empty);
            var machineGroupResult = _mapper.Map<List<MachineGroupUserAutoCompleteDto>>(result);

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var data in machineGroupResult)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: $"MachineGroup User details was fetched.",
                module: "MachineGroup User");
            await _mediator.Publish(domainEvent, cancellationToken);

            return machineGroupResult;
        }
    }
}
