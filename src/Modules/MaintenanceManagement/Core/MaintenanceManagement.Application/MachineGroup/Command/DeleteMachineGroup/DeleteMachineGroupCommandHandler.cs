using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup
{
    public class DeleteMachineGroupCommandHandler  : IRequestHandler<DeleteMachineGroupCommand, bool>
    {
        
         private readonly IMachineGroupCommandRepository  _machineGroupRepository;
         private readonly IMachineGroupQueryRepository  _machineGroupQueryRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;


         public DeleteMachineGroupCommandHandler(IMachineGroupCommandRepository machineGroupRepository, IMachineGroupQueryRepository machineGroupQueryRepository, IMapper imapper, IMediator mediator)
        {
            _machineGroupRepository = machineGroupRepository;
            _machineGroupQueryRepository = machineGroupQueryRepository;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMachineGroupCommand request, CancellationToken cancellationToken)
        {

             // Map the request to the entity
            var machineGroup = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineGroup>(request);

            // Perform the delete operation
            var isDeleted = await _machineGroupRepository.DeleteAsync(request.Id, machineGroup);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: machineGroup.Id.ToString(),
                actionName: machineGroup.IsDeleted.ToString(),
                details: $"MachineGroup with ID {machineGroup.Id} was deleted.",
                module: "MachineGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);


            return isDeleted==true ? isDeleted : throw new ExceptionRules("Machine Group deletion Failed.");
        }
    }
}