using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig
{
    public class UpdateMovementTypeConfigCommandHandler : IRequestHandler<UpdateMovementTypeConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IMovementTypeConfigCommandRepository _commandRepository;
        private readonly IMovementTypeConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMovementTypeConfigCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateMovementTypeConfigCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsMovementTypeConfigLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.MovementTypeConfig>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MOVEMENT_TYPE_CONFIG_UPDATE",
                actionName: request.Id.ToString(),
                details: $"MovementTypeConfig with Id {request.Id} updated successfully.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Movement Type Configuration updated successfully.",
                Data = result
            };
        }
    }
}
