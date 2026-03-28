using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetails
{
    public class GetInvoicePrintDetailsQueryHandler : IRequestHandler<GetInvoicePrintDetailsQuery, InvoicePrintDto?>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetInvoicePrintDetailsQueryHandler(IInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<InvoicePrintDto?> Handle(GetInvoicePrintDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPrintDetailsAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPrintDetails",
                actionCode: "INVOICE_PRINT",
                actionName: request.Id.ToString(),
                details: $"Invoice print details {request.Id} was fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
