using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateAutoComplete
{
    public sealed record GetRecurringJournalTemplateAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<RecurringJournalTemplateLookupDto>>;
}
