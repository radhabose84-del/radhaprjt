using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryAutoComplete
{
    public class GetSalesEnquiryAutoCompleteQueryHandler
        : IRequestHandler<GetSalesEnquiryAutoCompleteQuery, IReadOnlyList<SalesEnquiryLookupDto>>
    {
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesEnquiryAutoCompleteQueryHandler(
            ISalesEnquiryQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesEnquiryLookupDto>> Handle(
            GetSalesEnquiryAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesEnquiryAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "SalesEnquiry details was fetched.",
                module: "SalesEnquiry"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
