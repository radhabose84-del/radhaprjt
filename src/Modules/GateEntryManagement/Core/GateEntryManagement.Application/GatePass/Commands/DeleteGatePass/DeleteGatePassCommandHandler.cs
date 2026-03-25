using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.DeleteGatePass
{
    public class DeleteGatePassCommandHandler : IRequestHandler<DeleteGatePassCommand, bool>
    {
        private readonly IGatePassCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGatePassCommandHandler(
            IGatePassCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGatePassCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Gate Pass not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GATEPASS_DELETE",
                actionName: request.Id.ToString(),
                details: $"Gate Pass with Id {request.Id} soft-deleted successfully.",
                module: "GatePass"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
