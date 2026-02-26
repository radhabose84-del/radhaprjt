using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig
{
    public sealed class DeleteAgentCommissionConfigCommandHandler
        : IRequestHandler<DeleteAgentCommissionConfigCommand, bool>
    {
        private readonly IAgentCommissionConfigCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteAgentCommissionConfigCommandHandler(
            IAgentCommissionConfigCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteAgentCommissionConfigCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "AGENT_COMMISSION_CONFIG_DELETE",
                actionName: request.Id.ToString(),
                details: $"Agent Commission Configuration with Id {request.Id} soft deleted.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
