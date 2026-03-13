using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping
{
    public class DeleteAgentCustomerMappingCommandHandler
        : IRequestHandler<DeleteAgentCustomerMappingCommand, bool>
    {
        private readonly IAgentCustomerMappingCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteAgentCustomerMappingCommandHandler(
            IAgentCustomerMappingCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteAgentCustomerMappingCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Agent Customer Mapping not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "AGENT_CUSTOMER_MAPPING_DELETE",
                actionName: request.Id.ToString(),
                details: $"Agent Customer Mapping with Id {request.Id} deleted successfully.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
