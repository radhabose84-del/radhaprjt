using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions
{
    public class GetAgentCommissionsQueryHandler : IRequestHandler<GetAgentCommissionsQuery, List<AgentCommissionsDto>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAgentCommissionsQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<AgentCommissionsDto>> Handle(GetAgentCommissionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetAgentCommissionsAsync(
                request.SalesGroupId, request.PaymentTermId, request.AgentId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAgentCommissionsQuery",
                actionCode: "Get",
                actionName: $"SalesGroupId:{request.SalesGroupId},PaymentTermId:{request.PaymentTermId},AgentId:{request.AgentId}",
                details: $"Agent commissions for SalesGroup {request.SalesGroupId} + PaymentTerm {request.PaymentTermId} + Agent {request.AgentId} were fetched.",
                module: "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
