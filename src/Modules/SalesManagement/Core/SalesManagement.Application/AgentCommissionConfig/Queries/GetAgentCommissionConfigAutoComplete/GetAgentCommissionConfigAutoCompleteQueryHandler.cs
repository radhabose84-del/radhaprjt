using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigAutoComplete
{
    public class GetAgentCommissionConfigAutoCompleteQueryHandler
        : IRequestHandler<GetAgentCommissionConfigAutoCompleteQuery, IReadOnlyList<AgentCommissionConfigLookupDto>>
    {
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCommissionConfigAutoCompleteQueryHandler(
            IAgentCommissionConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AgentCommissionConfigLookupDto>> Handle(
            GetAgentCommissionConfigAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<AgentCommissionConfigLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAgentCommissionConfigAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "AgentCommissionConfig details was fetched.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
