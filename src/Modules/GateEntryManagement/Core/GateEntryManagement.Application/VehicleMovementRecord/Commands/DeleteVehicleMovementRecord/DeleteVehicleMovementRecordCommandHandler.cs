using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord
{
    public class DeleteVehicleMovementRecordCommandHandler : IRequestHandler<DeleteVehicleMovementRecordCommand, bool>
    {
        private readonly IVehicleMovementRecordCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteVehicleMovementRecordCommandHandler(
            IVehicleMovementRecordCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVehicleMovementRecordCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Vehicle Movement Record not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "VMR_DELETE",
                actionName: request.Id.ToString(),
                details: $"Vehicle Movement Record with Id {request.Id} soft-deleted successfully.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
