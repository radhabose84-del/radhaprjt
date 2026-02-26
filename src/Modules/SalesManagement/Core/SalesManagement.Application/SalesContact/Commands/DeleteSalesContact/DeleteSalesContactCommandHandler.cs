using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Commands.DeleteSalesContact
{
    public class DeleteSalesContactCommandHandler : IRequestHandler<DeleteSalesContactCommand, bool>
    {
        private readonly ISalesContactCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesContactCommandHandler(
            ISalesContactCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesContactCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Contact not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_CONTACT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Contact with Id {request.Id} soft deleted.",
                module: "SalesContact"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
