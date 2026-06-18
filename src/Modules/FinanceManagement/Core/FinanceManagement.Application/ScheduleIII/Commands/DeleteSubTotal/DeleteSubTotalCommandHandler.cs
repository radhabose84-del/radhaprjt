using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteSubTotal
{
    public class DeleteSubTotalCommandHandler : IRequestHandler<DeleteSubTotalCommand, bool>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSubTotalCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSubTotalCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteSubTotalAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("Sub-total not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "S3_SUBTOTAL_DELETE",
                actionName: request.Id.ToString(),
                details: $"Schedule III sub-total with Id {request.Id} deleted successfully.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
