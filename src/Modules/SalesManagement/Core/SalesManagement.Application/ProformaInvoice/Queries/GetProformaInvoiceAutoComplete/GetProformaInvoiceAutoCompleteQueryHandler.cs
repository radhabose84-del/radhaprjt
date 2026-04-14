using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceAutoComplete
{
    public class GetProformaInvoiceAutoCompleteQueryHandler : IRequestHandler<GetProformaInvoiceAutoCompleteQuery, IReadOnlyList<ProformaInvoiceLookupDto>>
    {
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProformaInvoiceAutoCompleteQueryHandler(IProformaInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ProformaInvoiceLookupDto>> Handle(GetProformaInvoiceAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetProformaInvoiceAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "ProformaInvoice details was fetched.",
                module: "ProformaInvoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
