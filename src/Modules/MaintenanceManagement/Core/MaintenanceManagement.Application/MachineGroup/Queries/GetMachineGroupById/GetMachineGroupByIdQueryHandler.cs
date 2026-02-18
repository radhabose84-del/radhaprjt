using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetMachineGroupByIdQueryHandler  : IRequestHandler<GetMachineGroupByIdQuery, GetMachineGroupByIdDto>
    {

        private readonly IMachineGroupQueryRepository  _machineGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        

        public GetMachineGroupByIdQueryHandler(IMachineGroupQueryRepository machineGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _machineGroupQueryRepository = machineGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
           
        } 

         public async Task<GetMachineGroupByIdDto> Handle(GetMachineGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _machineGroupQueryRepository.GetByIdAsync(request.Id);
            
          
            
            var machineGroup = _mapper.Map<GetMachineGroupByIdDto>(result);
            

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"MachineGroup details {machineGroup.Id} were fetched.",
                module: "MachineGroup"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return machineGroup;
        }



    }
}