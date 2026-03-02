using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Domain.Events;
using Contracts.Common;

namespace SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent
{
    public sealed class DeleteOfficerAgentCommandHandler
        : IRequestHandler<DeleteOfficerAgentCommand, bool>
    {
        private readonly IOfficerAgentCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteOfficerAgentCommandHandler(
            IOfficerAgentCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteOfficerAgentCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.DeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules("Officer Agent assignment not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "OFFICER_AGENT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Officer Agent assignment with Id {request.Id} deleted successfully.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
