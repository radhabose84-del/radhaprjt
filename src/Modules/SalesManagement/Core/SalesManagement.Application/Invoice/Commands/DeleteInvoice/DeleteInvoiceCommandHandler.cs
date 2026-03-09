using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Commands.DeleteInvoice
{
    public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, bool>
    {
        private readonly IInvoiceCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteInvoiceCommandHandler(
            IInvoiceCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Invoice not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "INVOICE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Invoice with Id {request.Id} deleted successfully.",
                module: "Invoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
