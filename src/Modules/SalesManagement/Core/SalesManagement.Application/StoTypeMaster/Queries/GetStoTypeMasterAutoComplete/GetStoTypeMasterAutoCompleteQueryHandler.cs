using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterAutoComplete
{
    public class GetStoTypeMasterAutoCompleteQueryHandler : IRequestHandler<GetStoTypeMasterAutoCompleteQuery, IReadOnlyList<StoTypeMasterLookupDto>>
    {
        private readonly IStoTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoTypeMasterAutoCompleteQueryHandler(IStoTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<StoTypeMasterLookupDto>> Handle(GetStoTypeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<StoTypeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStoTypeMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "StoTypeMaster details was fetched.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
