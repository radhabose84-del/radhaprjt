using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster
{
    public class DeleteSalesOrderTypeMasterCommandHandler
        : IRequestHandler<DeleteSalesOrderTypeMasterCommand, bool>
    {
        private readonly ISalesOrderTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesOrderTypeMasterCommandHandler(
            ISalesOrderTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteSalesOrderTypeMasterCommand request,
            CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALESORDERTYPEMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Order Type with Id {request.Id} soft deleted.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
