using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Commands.DeleteSalesLead
{
    public sealed class DeleteSalesLeadCommandHandler : IRequestHandler<DeleteSalesLeadCommand, bool>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesLeadCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_LEAD_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Lead with Id {request.Id} soft deleted.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
