using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerAutoComplete
{
    public class GetMarketingOfficerAutoCompleteQueryHandler : IRequestHandler<GetMarketingOfficerAutoCompleteQuery, IReadOnlyList<MarketingOfficerLookupDto>>
    {
        private readonly IMarketingOfficerQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMarketingOfficerAutoCompleteQueryHandler(IMarketingOfficerQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MarketingOfficerLookupDto>> Handle(GetMarketingOfficerAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMarketingOfficerAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "MarketingOfficer details was fetched.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
