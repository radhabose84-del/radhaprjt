using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementAutoComplete
{
    public class GetSalesAgreementAutoCompleteQueryHandler : IRequestHandler<GetSalesAgreementAutoCompleteQuery, IReadOnlyList<SalesAgreementLookupDto>>
    {
        private readonly ISalesAgreementQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesAgreementAutoCompleteQueryHandler(
            ISalesAgreementQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesAgreementLookupDto>> Handle(GetSalesAgreementAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesAgreementAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Sales Agreement details was fetched.",
                module: "SalesAgreement");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
