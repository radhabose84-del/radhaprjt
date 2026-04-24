using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetAutoComplete
{
    public class GetTripSheetAutoCompleteQueryHandler : IRequestHandler<GetTripSheetAutoCompleteQuery, IReadOnlyList<TripSheetLookupDto>>
    {
        private readonly ITripSheetQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTripSheetAutoCompleteQueryHandler(ITripSheetQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<TripSheetLookupDto>> Handle(GetTripSheetAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetTripSheetAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "TripSheet details was fetched.",
                module: "TripSheet"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
