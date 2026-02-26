using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel
{
    public sealed class DeleteSalesChannelCommandHandler
        : IRequestHandler<DeleteSalesChannelCommand, bool>
    {
        private readonly ISalesChannelCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesChannelCommandHandler(
            ISalesChannelCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesChannelCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_CHANNEL_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Channel with Id {request.Id} soft deleted.",
                module: "SalesChannel"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
