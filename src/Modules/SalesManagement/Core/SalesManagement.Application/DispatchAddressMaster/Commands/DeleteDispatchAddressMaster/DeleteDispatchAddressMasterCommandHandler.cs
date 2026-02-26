using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster
{
    public sealed class DeleteDispatchAddressMasterCommandHandler
        : IRequestHandler<DeleteDispatchAddressMasterCommand, bool>
    {
        private readonly IDispatchAddressMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteDispatchAddressMasterCommandHandler(
            IDispatchAddressMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDispatchAddressMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DISPATCH_ADDRESS_DELETE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Address Master with Id {request.Id} soft deleted.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
