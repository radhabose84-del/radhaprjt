using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit
{
    public class DeleteCustomerVisitCommandHandler : IRequestHandler<DeleteCustomerVisitCommand, bool>
    {
        private readonly ICustomerVisitCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCustomerVisitCommandHandler(
            ICustomerVisitCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCustomerVisitCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("CustomerVisit not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "CUSTOMER_VISIT_DELETE",
                actionName: request.Id.ToString(),
                details: $"CustomerVisit with Id {request.Id} was soft-deleted.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
