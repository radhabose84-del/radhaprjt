using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder
{
    public class ForecloseSalesOrderCommandHandler : IRequestHandler<ForecloseSalesOrderCommand, bool>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ForecloseSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ForecloseSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.ForecloseAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Order not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Foreclose",
                actionCode: "SALESORDER_FORECLOSE",
                actionName: request.Id.ToString(),
                details: $"Sales Order with Id {request.Id} foreclosed successfully.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
