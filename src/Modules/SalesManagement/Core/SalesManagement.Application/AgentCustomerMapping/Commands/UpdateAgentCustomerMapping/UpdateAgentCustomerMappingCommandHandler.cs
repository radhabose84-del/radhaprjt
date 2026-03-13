using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping
{
    public class UpdateAgentCustomerMappingCommandHandler
        : IRequestHandler<UpdateAgentCustomerMappingCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCustomerMappingCommandRepository _commandRepository;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAgentCustomerMappingCommandHandler(
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
            UpdateAgentCustomerMappingCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AgentCustomerMapping>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "AGENT_CUSTOMER_MAPPING_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Agent Customer Mapping with Id {request.Id} updated successfully.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Customer Mapping updated successfully.",
                Data = result
            };
        }
    }
}
