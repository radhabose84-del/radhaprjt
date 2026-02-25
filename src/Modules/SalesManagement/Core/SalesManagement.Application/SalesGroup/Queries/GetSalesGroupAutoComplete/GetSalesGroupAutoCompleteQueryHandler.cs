#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete
{
    public class GetSalesGroupAutoCompleteQueryHandler : IRequestHandler<GetSalesGroupAutoCompleteQuery, IReadOnlyList<SalesGroupLookupDto>>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesGroupAutoCompleteQueryHandler(ISalesGroupQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesGroupLookupDto>> Handle(GetSalesGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesGroups = _mapper.Map<List<SalesGroupLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesGroupAutoCompleteQuery",
                actionName: salesGroups.Count.ToString(),
                details: "SalesGroup details was fetched.",
                module: "SalesGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesGroups;
        }
    }
}
