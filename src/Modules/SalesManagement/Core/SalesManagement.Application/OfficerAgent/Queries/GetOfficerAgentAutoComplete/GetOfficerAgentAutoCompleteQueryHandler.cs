using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentAutoComplete
{
    public class GetOfficerAgentAutoCompleteQueryHandler
        : IRequestHandler<GetOfficerAgentAutoCompleteQuery, IReadOnlyList<OfficerAgentLookupDto>>
    {
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetOfficerAgentAutoCompleteQueryHandler(
            IOfficerAgentQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<OfficerAgentLookupDto>> Handle(
            GetOfficerAgentAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var dtos = _mapper.Map<List<OfficerAgentLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetOfficerAgentAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "OfficerAgent details was fetched.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
