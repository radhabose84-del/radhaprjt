using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrder
{
    public class DeleteSalesOrderCommandHandler : IRequestHandler<DeleteSalesOrderCommand, bool>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            ISalesOrderQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Order not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALESORDER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Order with Id {request.Id} soft-deleted successfully.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
