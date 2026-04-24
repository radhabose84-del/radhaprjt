using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetInvoicePrintDetails
{
    public class GetTripSheetInvoicePrintDetailsQueryHandler : IRequestHandler<GetTripSheetInvoicePrintDetailsQuery, List<InvoicePrintDto>>
    {
        private readonly IInvoiceQueryRepository _invoiceQueryRepository;
        private readonly IMediator _mediator;

        public GetTripSheetInvoicePrintDetailsQueryHandler(
            IInvoiceQueryRepository invoiceQueryRepository,
            IMediator mediator)
        {
            _invoiceQueryRepository = invoiceQueryRepository;
            _mediator = mediator;
        }

        public async Task<List<InvoicePrintDto>> Handle(GetTripSheetInvoicePrintDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _invoiceQueryRepository.GetPrintDetailsByTripSheetAsync(request.TripSheetHeaderId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTripSheetInvoicePrintDetails",
                actionCode: "GetTripSheetInvoicePrintDetailsQuery",
                actionName: result.Count.ToString(),
                details: $"Invoice print details for TripSheet {request.TripSheetHeaderId} were fetched.",
                module: "TripSheet"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
