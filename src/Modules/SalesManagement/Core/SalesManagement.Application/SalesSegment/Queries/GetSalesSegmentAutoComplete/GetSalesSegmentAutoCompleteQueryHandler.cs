using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentAutoComplete
{
    public class GetSalesSegmentAutoCompleteQueryHandler : IRequestHandler<GetSalesSegmentAutoCompleteQuery, IReadOnlyList<SalesSegmentLookupDto>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesSegmentAutoCompleteQueryHandler(ISalesSegmentQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesSegmentLookupDto>> Handle(GetSalesSegmentAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var salesSegments = _mapper.Map<List<SalesSegmentLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesSegmentAutoCompleteQuery",
                actionName: salesSegments.Count.ToString(),
                details: "SalesSegment details was fetched.",
                module: "SalesSegment"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesSegments;
        }
    }
}
