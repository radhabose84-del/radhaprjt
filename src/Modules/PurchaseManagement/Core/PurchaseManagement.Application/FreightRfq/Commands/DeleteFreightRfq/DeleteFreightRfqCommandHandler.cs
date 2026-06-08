using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.DeleteFreightRfq
{
    public class DeleteFreightRfqCommandHandler : IRequestHandler<DeleteFreightRfqCommand, bool>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("Freight RFQ not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "FREIGHTRFQ_DELETE",
                actionName: request.Id.ToString(),
                details: $"Freight RFQ {request.Id} deleted successfully.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
