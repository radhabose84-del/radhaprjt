using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping
{
    public class CreateAgentCustomerMappingCommandHandler
        : IRequestHandler<CreateAgentCustomerMappingCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCustomerMappingCommandRepository _commandRepository;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAgentCustomerMappingCommandHandler(
            IAgentCustomerMappingCommandRepository commandRepository,
            IAgentCustomerMappingQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateAgentCustomerMappingCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AgentCustomerMapping>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "AGENT_CUSTOMER_MAPPING_CREATE",
                actionName: newId.ToString(),
                details: $"Agent Customer Mapping created successfully with Id {newId}.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Customer Mapping created successfully.",
                Data = newId
            };
        }
    }
}
