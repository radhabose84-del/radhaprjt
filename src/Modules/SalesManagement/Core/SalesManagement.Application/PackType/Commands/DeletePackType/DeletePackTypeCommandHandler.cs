using MediatR;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.PackType.Commands.DeletePackType
{
    public sealed class DeletePackTypeCommandHandler
        : IRequestHandler<DeletePackTypeCommand, bool>
    {
        private readonly IPackTypeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeletePackTypeCommandHandler(
            IPackTypeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeletePackTypeCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PACKTYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"PackType with Id {request.Id} soft deleted.",
                module: "PackType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
