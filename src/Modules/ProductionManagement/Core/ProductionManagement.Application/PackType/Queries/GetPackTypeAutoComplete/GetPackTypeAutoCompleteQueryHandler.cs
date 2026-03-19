using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.PackType.Queries.GetPackTypeAutoComplete
{
    public class GetPackTypeAutoCompleteQueryHandler : IRequestHandler<GetPackTypeAutoCompleteQuery, IReadOnlyList<PackTypeLookupDto>>
    {
        private readonly IPackTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPackTypeAutoCompleteQueryHandler(IPackTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<PackTypeLookupDto>> Handle(GetPackTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var packTypes = _mapper.Map<List<PackTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPackTypeAutoCompleteQuery",
                actionName: packTypes.Count.ToString(),
                details: "PackType details was fetched.",
                module: "PackType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return packTypes;
        }
    }
}
