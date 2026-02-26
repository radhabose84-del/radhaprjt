using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment
{
    public class DeleteSalesSegmentCommandHandler : IRequestHandler<DeleteSalesSegmentCommand, bool>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesSegmentCommandHandler(
            ISalesSegmentCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_SEGMENT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Segment with Id {request.Id} soft deleted.",
                module: "SalesSegment"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
