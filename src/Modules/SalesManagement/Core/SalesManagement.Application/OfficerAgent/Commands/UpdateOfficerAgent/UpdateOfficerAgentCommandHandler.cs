using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent
{
    public class UpdateOfficerAgentCommandHandler
        : IRequestHandler<UpdateOfficerAgentCommand, ApiResponseDTO<int>>
    {
        private readonly IOfficerAgentCommandRepository _commandRepository;
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateOfficerAgentCommandHandler(
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
            UpdateOfficerAgentCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.OfficerAgent>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "OFFICER_AGENT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Officer Agent assignment with Id {request.Id} updated successfully.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Officer Agent assignment updated successfully.",
                Data = result
            };
        }
    }
}
