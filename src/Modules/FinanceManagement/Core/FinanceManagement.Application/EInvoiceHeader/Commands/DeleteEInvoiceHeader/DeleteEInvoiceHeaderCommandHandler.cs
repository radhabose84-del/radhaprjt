using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader
{
    public class DeleteEInvoiceHeaderCommandHandler : IRequestHandler<DeleteEInvoiceHeaderCommand, bool>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteEInvoiceHeaderCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteEInvoiceHeaderCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "EINVOICE_HEADER_DELETE",
                actionName: request.Id.ToString(),
                details: $"EInvoice Header with Id {request.Id} soft deleted.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
