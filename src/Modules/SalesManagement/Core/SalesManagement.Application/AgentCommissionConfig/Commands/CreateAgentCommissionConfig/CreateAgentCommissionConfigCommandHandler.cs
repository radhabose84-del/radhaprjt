using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig
{
    public class CreateAgentCommissionConfigCommandHandler
        : IRequestHandler<CreateAgentCommissionConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCommissionConfigCommandRepository _commandRepository;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAgentCommissionConfigCommandHandler(
            IAgentCommissionConfigCommandRepository commandRepository,
            IAgentCommissionConfigQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateAgentCommissionConfigCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AgentCommissionConfig>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "AGENT_COMMISSION_CONFIG_CREATE",
                actionName: newId.ToString(),
                details: $"Agent Commission Configuration created successfully with Id {newId}.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Commission Configuration created successfully.",
                Data = newId
            };
        }
    }
}
