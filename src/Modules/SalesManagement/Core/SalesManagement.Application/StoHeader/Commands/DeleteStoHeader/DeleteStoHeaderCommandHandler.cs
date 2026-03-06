using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Commands.DeleteStoHeader
{
    public class DeleteStoHeaderCommandHandler : IRequestHandler<DeleteStoHeaderCommand, bool>
    {
        private readonly IStoHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteStoHeaderCommandHandler(
            IStoHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteStoHeaderCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "STO_HEADER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Stock Transfer Order with Id {request.Id} soft deleted.",
                module: "StoHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
