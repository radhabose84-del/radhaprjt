using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingAutoComplete
{
    public class GetAgentCustomerMappingAutoCompleteQueryHandler
        : IRequestHandler<GetAgentCustomerMappingAutoCompleteQuery, IReadOnlyList<AgentCustomerMappingLookupDto>>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCustomerMappingAutoCompleteQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AgentCustomerMappingLookupDto>> Handle(
            GetAgentCustomerMappingAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var dtos = _mapper.Map<List<AgentCustomerMappingLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAgentCustomerMappingAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "AgentCustomerMapping details was fetched.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
