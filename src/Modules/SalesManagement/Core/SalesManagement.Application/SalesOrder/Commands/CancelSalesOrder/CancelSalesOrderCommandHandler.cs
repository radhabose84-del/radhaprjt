using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.CancelSalesOrder
{
    public class CancelSalesOrderCommandHandler : IRequestHandler<CancelSalesOrderCommand, bool>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public CancelSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.CancelAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Order not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Cancel",
                actionCode: "SALESORDER_CANCEL",
                actionName: request.Id.ToString(),
                details: $"Sales Order with Id {request.Id} cancelled successfully.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
