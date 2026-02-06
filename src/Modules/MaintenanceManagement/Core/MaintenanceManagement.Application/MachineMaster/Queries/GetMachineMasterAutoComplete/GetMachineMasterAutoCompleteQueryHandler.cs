using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete
{
    public class GetMachineMasterAutoCompleteQueryHandler : IRequestHandler<GetMachineMasterAutoCompleteQuery,List<MachineMasterAutoCompleteDto>>
    {
        private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetMachineMasterAutoCompleteQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _imachineMasterQueryRepository = imachineMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<List<MachineMasterAutoCompleteDto>> Handle(GetMachineMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _imachineMasterQueryRepository.GetMachineAsync(request.SearchPattern);
            var machineMasters = _mapper.Map<List<MachineMasterAutoCompleteDto>>(result);

            // Fetch departments using lookup
            var departments = await _departmentLookup.GetAllDepartmentAsync();

            // Create a list of valid DepartmentIds
            var validDepartmentIds = departments.Select(d => d.DepartmentId).ToHashSet();

            // Filter only machineMasters whose DepartmentId exists in the department list
            var filteredMachines = machineMasters
                .Where(m => validDepartmentIds.Contains(m.DepartmentId))
                .ToList();

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetMachineMasterAutoCompleteQuery",
                    actionName: filteredMachines.Count.ToString(),
                    details: $"MachineMaster details was fetched.",
                    module:"MachineMaster"
                );
            await _mediator.Publish(domainEvent, cancellationToken);
            return filteredMachines;
        }
    }
}