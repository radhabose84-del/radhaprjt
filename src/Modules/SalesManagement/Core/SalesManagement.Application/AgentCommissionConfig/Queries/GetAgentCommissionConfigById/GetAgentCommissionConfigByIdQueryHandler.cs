using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigById
{
    public class GetAgentCommissionConfigByIdQueryHandler
        : IRequestHandler<GetAgentCommissionConfigByIdQuery, AgentCommissionConfigDto?>
    {
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCommissionConfigByIdQueryHandler(
            IAgentCommissionConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AgentCommissionConfigDto?> Handle(
            GetAgentCommissionConfigByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<AgentCommissionConfigDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetAgentCommissionConfigByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"AgentCommissionConfig details {dto.Id} was fetched.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
