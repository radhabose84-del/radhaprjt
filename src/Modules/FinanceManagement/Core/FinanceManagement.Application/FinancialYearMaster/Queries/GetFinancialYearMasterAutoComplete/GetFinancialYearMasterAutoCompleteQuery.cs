using FinanceManagement.Application.FinancialYearMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterAutoComplete
{
    public sealed record GetFinancialYearMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<FinancialYearMasterLookupDto>>;
}
