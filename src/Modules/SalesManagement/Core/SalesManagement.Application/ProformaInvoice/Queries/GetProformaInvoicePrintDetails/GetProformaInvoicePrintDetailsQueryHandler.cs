using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoicePrintDetails
{
    public class GetProformaInvoicePrintDetailsQueryHandler
        : IRequestHandler<GetProformaInvoicePrintDetailsQuery, ProformaInvoicePrintDto?>
    {
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProformaInvoicePrintDetailsQueryHandler(
            IProformaInvoiceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProformaInvoicePrintDto?> Handle(
            GetProformaInvoicePrintDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPrintDetailsAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPrintDetails",
                actionCode: "PROFORMA_INVOICE_PRINT",
                actionName: request.Id.ToString(),
                details: $"Proforma Invoice print details for Id {request.Id} was fetched.",
                module: "ProformaInvoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
