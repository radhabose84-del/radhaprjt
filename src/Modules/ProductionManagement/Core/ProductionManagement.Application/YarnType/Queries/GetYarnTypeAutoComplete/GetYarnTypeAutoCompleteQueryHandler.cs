using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Queries.GetYarnTypeAutoComplete
{
    public class GetYarnTypeAutoCompleteQueryHandler : IRequestHandler<GetYarnTypeAutoCompleteQuery, IReadOnlyList<YarnTypeLookupDto>>
    {
        private readonly IYarnTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnTypeAutoCompleteQueryHandler(
            IYarnTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<YarnTypeLookupDto>> Handle(GetYarnTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<YarnTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetYarnTypeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Yarn Type details was fetched.",
                module: "YarnType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
