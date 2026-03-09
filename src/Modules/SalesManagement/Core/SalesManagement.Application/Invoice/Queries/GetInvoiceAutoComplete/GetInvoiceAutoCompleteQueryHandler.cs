using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete
{
    public class GetInvoiceAutoCompleteQueryHandler : IRequestHandler<GetInvoiceAutoCompleteQuery, IReadOnlyList<InvoiceLookupDto>>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetInvoiceAutoCompleteQueryHandler(IInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<InvoiceLookupDto>> Handle(GetInvoiceAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetInvoiceAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Invoice details was fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
