using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId
{
    public class GetMachineDepartmentbyIdQueryHandler : IRequestHandler<GetMachineDepartmentbyIdQuery, MachineDepartmentGroupDto>
    {
        private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMachineDepartmentbyIdQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imachineMasterQueryRepository = imachineMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MachineDepartmentGroupDto> Handle(GetMachineDepartmentbyIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _imachineMasterQueryRepository.GetMachineDepartment(request.MachineGroupId);
         
            // Map a single entity
            var machineMaster = _mapper.Map<MachineDepartmentGroupDto>(result);
           
          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "GetMachineDepartmentbyIdQuery",        
                    actionName: machineMaster.DepartmentName.ToString(),
                    details: $"Machine department details {machineMaster.DepartmentGroupName} was fetched.",
                    module:"MachineDepartment"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return machineMaster;
        }
    }
}