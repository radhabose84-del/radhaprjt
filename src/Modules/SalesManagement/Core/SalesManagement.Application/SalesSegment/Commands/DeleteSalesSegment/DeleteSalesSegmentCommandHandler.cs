#nullable disable

using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment
{
    public class DeleteSalesSegmentCommandHandler : IRequestHandler<DeleteSalesSegmentCommand, bool>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteSalesSegmentCommandHandler(
            ISalesSegmentCommandRepository commandRepository,
            ISalesSegmentQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            // Get entity details for audit log
            var entity = await _queryRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return false;
            }

            // Soft delete
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (result)
            {
                // Publish audit event
                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "SoftDelete",
                    actionCode: "SALES_SEGMENT_DELETE",
                    actionName: entity.SegmentName,
                    details: $"Sales Segment Id {request.Id} soft deleted.",
                    module: "SalesSegment"
                );
                await _mediator.Publish(auditEvent, cancellationToken);
            }

            return result;
        }
    }
}
