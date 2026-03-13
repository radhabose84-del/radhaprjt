using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingById
{
    public class GetAgentCustomerMappingByIdQueryHandler
        : IRequestHandler<GetAgentCustomerMappingByIdQuery, AgentCustomerMappingDto?>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCustomerMappingByIdQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AgentCustomerMappingDto?> Handle(
            GetAgentCustomerMappingByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<AgentCustomerMappingDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetAgentCustomerMappingByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"AgentCustomerMapping details {dto.Id} was fetched.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
