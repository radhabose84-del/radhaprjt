using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent
{
    public class CreateOfficerAgentCommandHandler
        : IRequestHandler<CreateOfficerAgentCommand, ApiResponseDTO<int>>
    {
        private readonly IOfficerAgentCommandRepository _commandRepository;
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateOfficerAgentCommandHandler(
            IOfficerAgentCommandRepository commandRepository,
            IOfficerAgentQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateOfficerAgentCommand request,
            CancellationToken cancellationToken)
        {
            var entities = _mapper.Map<List<Domain.Entities.OfficerAgent>>(request);

            var count = await _commandRepository.CreateBatchAsync(entities);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "OFFICER_AGENT_CREATE",
                actionName: request.MarketingOfficerId.ToString(),
                details: $"{count} Officer Agent assignment(s) created for officer Id {request.MarketingOfficerId}.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{count} Officer Agent assignment(s) created successfully.",
                Data = count
            };
        }
    }
}
