using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster
{
    public class DeleteStoTypeMasterCommandHandler : IRequestHandler<DeleteStoTypeMasterCommand, bool>
    {
        private readonly IStoTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteStoTypeMasterCommandHandler(
            IStoTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteStoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "STO_TYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"STO Type Master with Id {request.Id} soft deleted.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
