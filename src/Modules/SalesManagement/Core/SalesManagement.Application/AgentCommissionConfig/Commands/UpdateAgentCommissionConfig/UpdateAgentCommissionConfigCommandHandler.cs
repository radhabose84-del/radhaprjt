using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig
{
    public class UpdateAgentCommissionConfigCommandHandler
        : IRequestHandler<UpdateAgentCommissionConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCommissionConfigCommandRepository _commandRepository;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAgentCommissionConfigCommandHandler(
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
            UpdateAgentCommissionConfigCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AgentCommissionConfig>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "AGENT_COMMISSION_CONFIG_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Agent Commission Configuration with Id {request.Id} updated successfully.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Commission Configuration updated successfully.",
                Data = updatedId
            };
        }
    }
}
