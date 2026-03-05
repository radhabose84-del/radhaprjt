using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Queries.GetStoHeaderAutoComplete
{
    public class GetStoHeaderAutoCompleteQueryHandler : IRequestHandler<GetStoHeaderAutoCompleteQuery, IReadOnlyList<StoHeaderLookupDto>>
    {
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoHeaderAutoCompleteQueryHandler(IStoHeaderQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<StoHeaderLookupDto>> Handle(GetStoHeaderAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<StoHeaderLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStoHeaderAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "StoHeader details was fetched.",
                module: "StoHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
