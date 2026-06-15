using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem
{
    public class DeleteLineItemCommandHandler : IRequestHandler<DeleteLineItemCommand, bool>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteLineItemCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteLineItemCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteLineItemAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Line item not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "S3_LINEITEM_DELETE",
                actionName: request.Id.ToString(),
                details: $"Schedule III line item with Id {request.Id} deleted successfully.",
                module: "ScheduleIIILineItem"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
