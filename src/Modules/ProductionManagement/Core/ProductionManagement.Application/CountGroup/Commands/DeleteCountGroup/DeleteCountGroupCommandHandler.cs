using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup
{
    public class DeleteCountGroupCommandHandler : IRequestHandler<DeleteCountGroupCommand, bool>
    {
        private readonly ICountGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCountGroupCommandHandler(
            ICountGroupCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCountGroupCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "COUNTGROUP_DELETE",
                actionName: request.Id.ToString(),
                details: $"Count Group with Id {request.Id} soft deleted.",
                module: "CountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
