using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupAutoComplete
{
    public sealed record GetAccountGroupAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<AccountGroupLookupDto>>;
}
