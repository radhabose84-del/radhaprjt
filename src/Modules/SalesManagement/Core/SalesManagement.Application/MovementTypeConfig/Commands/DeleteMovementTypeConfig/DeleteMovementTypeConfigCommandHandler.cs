using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig
{
    public sealed class DeleteMovementTypeConfigCommandHandler : IRequestHandler<DeleteMovementTypeConfigCommand, bool>
    {
        private readonly IMovementTypeConfigCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMovementTypeConfigCommandHandler(
            IMovementTypeConfigCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMovementTypeConfigCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MOVEMENT_TYPE_CONFIG_DELETE",
                actionName: request.Id.ToString(),
                details: $"MovementTypeConfig with Id {request.Id} soft deleted.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
