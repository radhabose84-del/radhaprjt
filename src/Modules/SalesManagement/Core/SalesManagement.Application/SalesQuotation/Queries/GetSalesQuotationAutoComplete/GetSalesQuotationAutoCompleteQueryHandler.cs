using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAutoComplete
{
    public class GetSalesQuotationAutoCompleteQueryHandler : IRequestHandler<GetSalesQuotationAutoCompleteQuery, IReadOnlyList<SalesQuotationLookupDto>>
    {
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesQuotationAutoCompleteQueryHandler(
            ISalesQuotationQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesQuotationLookupDto>> Handle(GetSalesQuotationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesQuotationAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Sales Quotation details was fetched.",
                module: "SalesQuotation");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
