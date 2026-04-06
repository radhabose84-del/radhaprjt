using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn
{
    public class DeleteSalesReturnCommandHandler : IRequestHandler<DeleteSalesReturnCommand, bool>
    {
        private readonly ISalesReturnCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesReturnCommandHandler(
            ISalesReturnCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesReturnCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Return not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_RETURN_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Return with Id {request.Id} deleted.",
                module: "SalesReturn");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
