using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.DeleteGateInward
{
    public class DeleteGateInwardCommandHandler : IRequestHandler<DeleteGateInwardCommand, bool>
    {
        private readonly IGateInwardCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGateInwardCommandHandler(IGateInwardCommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGateInwardCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!result)
                throw new ExceptionRules("Gate Inward Entry not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GATEINWARD_DELETE",
                actionName: request.Id.ToString(),
                details: $"Gate Inward Entry with Id {request.Id} soft-deleted successfully.",
                module: "GateInward"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
