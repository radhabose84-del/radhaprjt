using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Queries.GetCountGroupAutoComplete
{
    public class GetCountGroupAutoCompleteQueryHandler : IRequestHandler<GetCountGroupAutoCompleteQuery, IReadOnlyList<CountGroupLookupDto>>
    {
        private readonly ICountGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCountGroupAutoCompleteQueryHandler(
            ICountGroupQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CountGroupLookupDto>> Handle(GetCountGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<CountGroupLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCountGroupAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Count Group details was fetched.",
                module: "CountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
