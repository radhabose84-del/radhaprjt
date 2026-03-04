using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig
{
    public class CreateMovementTypeConfigCommandHandler : IRequestHandler<CreateMovementTypeConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IMovementTypeConfigCommandRepository _commandRepository;
        private readonly IMovementTypeConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMovementTypeConfigCommandHandler(
            IMovementTypeConfigCommandRepository commandRepository,
            IMovementTypeConfigQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMovementTypeConfigCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MovementTypeConfig>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MOVEMENT_TYPE_CONFIG_CREATE",
                actionName: request.MovementCode ?? string.Empty,
                details: $"MovementTypeConfig '{request.MovementCode}' created successfully with Id {newId}.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Movement Type Configuration created successfully.",
                Data = newId
            };
        }
    }
}
