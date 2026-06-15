using FinanceManagement.Application.AccountTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterAutoComplete
{
    public sealed record GetAccountTypeMasterAutoCompleteQuery(string Term, int? CompanyId)
        : IRequest<IReadOnlyList<AccountTypeMasterLookupDto>>;
}
