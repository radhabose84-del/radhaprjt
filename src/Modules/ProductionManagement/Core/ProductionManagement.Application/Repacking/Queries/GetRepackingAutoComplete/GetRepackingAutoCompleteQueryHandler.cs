using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Queries.GetRepackingAutoComplete
{
    public class GetRepackingAutoCompleteQueryHandler
        : IRequestHandler<GetRepackingAutoCompleteQuery, IReadOnlyList<RepackingLookupDto>>
    {
        private readonly IRepackingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingAutoCompleteQueryHandler(
            IRepackingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RepackingLookupDto>> Handle(
            GetRepackingAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRepackingAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Repacking details was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
