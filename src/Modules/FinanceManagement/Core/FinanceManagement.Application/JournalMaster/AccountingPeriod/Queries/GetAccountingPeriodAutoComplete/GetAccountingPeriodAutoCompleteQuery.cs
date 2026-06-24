using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodAutoComplete
{
    public sealed record GetAccountingPeriodAutoCompleteQuery(string Term, int? FinancialYearId = null)
        : IRequest<IReadOnlyList<AccountingPeriodLookupDto>>;
}
