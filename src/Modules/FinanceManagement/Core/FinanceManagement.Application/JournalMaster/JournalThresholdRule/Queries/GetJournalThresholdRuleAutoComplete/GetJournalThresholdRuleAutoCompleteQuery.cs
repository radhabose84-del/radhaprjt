using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleAutoComplete
{
    public sealed record GetJournalThresholdRuleAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<JournalThresholdRuleLookupDto>>;
}
