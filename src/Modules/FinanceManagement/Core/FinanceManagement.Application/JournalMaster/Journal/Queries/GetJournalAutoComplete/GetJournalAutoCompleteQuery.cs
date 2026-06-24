using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalAutoComplete
{
    // Autocomplete journal vouchers by VoucherNo / Narration, optionally filtered by JOURNAL_STATUS id.
    public sealed record GetJournalAutoCompleteQuery(string Term, int? StatusId = null)
        : IRequest<IReadOnlyList<JournalLookupDto>>;
}
