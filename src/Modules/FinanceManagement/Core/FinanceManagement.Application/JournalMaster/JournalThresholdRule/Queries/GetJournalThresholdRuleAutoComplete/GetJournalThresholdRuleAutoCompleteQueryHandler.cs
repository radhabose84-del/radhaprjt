using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleAutoComplete
{
    public class GetJournalThresholdRuleAutoCompleteQueryHandler : IRequestHandler<GetJournalThresholdRuleAutoCompleteQuery, IReadOnlyList<JournalThresholdRuleLookupDto>>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetJournalThresholdRuleAutoCompleteQueryHandler(IJournalThresholdRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<JournalThresholdRuleLookupDto>> Handle(GetJournalThresholdRuleAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var dtos = _mapper.Map<List<JournalThresholdRuleLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetJournalThresholdRuleAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Journal threshold rule details was fetched.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
