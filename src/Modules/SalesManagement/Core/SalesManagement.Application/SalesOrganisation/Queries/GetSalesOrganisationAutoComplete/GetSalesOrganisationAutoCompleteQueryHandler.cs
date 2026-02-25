#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationAutoComplete
{
    public class GetSalesOrganisationAutoCompleteQueryHandler : IRequestHandler<GetSalesOrganisationAutoCompleteQuery, IReadOnlyList<SalesOrganisationLookupDto>>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOrganisationAutoCompleteQueryHandler(ISalesOrganisationQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesOrganisationLookupDto>> Handle(GetSalesOrganisationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesOrganisations = _mapper.Map<List<SalesOrganisationLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesOrganisationAutoCompleteQuery",
                actionName: salesOrganisations.Count.ToString(),
                details: "SalesOrganisation details was fetched.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesOrganisations;
        }
    }
}
