using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetailsByIds
{
    public class GetInvoicePrintDetailsByIdsQueryHandler : IRequestHandler<GetInvoicePrintDetailsByIdsQuery, List<InvoicePrintDto>>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetInvoicePrintDetailsByIdsQueryHandler(
            IInvoiceQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<InvoicePrintDto>> Handle(GetInvoicePrintDetailsByIdsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPrintDetailsByIdsAsync(request.InvoiceIds, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetInvoicePrintDetailsByIds",
                actionCode: "INVOICE_PRINT_MULTIPLE",
                actionName: result.Count.ToString(),
                details: $"Invoice print details for {request.InvoiceIds.Count} invoice(s) were fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
