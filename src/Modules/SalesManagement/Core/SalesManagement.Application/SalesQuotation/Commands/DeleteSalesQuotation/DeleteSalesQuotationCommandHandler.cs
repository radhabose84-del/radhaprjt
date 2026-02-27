using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation
{
    public class DeleteSalesQuotationCommandHandler : IRequestHandler<DeleteSalesQuotationCommand, bool>
    {
        private readonly ISalesQuotationCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesQuotationCommandHandler(
            ISalesQuotationCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesQuotationCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Quotation not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALESQUOTATION_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Quotation with Id {request.Id} soft deleted.",
                module: "SalesQuotation");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
