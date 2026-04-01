using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete
{
    public class GetCountMasterAutoCompleteQueryHandler : IRequestHandler<GetCountMasterAutoCompleteQuery, IReadOnlyList<CountMasterLookupDto>>
    {
        private readonly ICountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCountMasterAutoCompleteQueryHandler(
            ICountMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CountMasterLookupDto>> Handle(GetCountMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<CountMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCountMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Count Master details was fetched.",
                module: "CountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
