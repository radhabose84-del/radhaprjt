using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation
{
    public sealed class DeleteSalesOrganisationCommandHandler
        : IRequestHandler<DeleteSalesOrganisationCommand, bool>
    {
        private readonly ISalesOrganisationCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesOrganisationCommandHandler(
            ISalesOrganisationCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOrganisationCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_ORG_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Organisation with Id {request.Id} soft deleted.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
