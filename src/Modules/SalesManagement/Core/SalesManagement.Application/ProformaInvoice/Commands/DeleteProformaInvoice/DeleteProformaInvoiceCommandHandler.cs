using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice
{
    public sealed class DeleteProformaInvoiceCommandHandler
        : IRequestHandler<DeleteProformaInvoiceCommand, bool>
    {
        private readonly IProformaInvoiceCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteProformaInvoiceCommandHandler(
            IProformaInvoiceCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteProformaInvoiceCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PROFORMA_DELETE",
                actionName: request.Id.ToString(),
                details: $"Proforma Invoice with Id {request.Id} soft deleted.",
                module: "ProformaInvoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
