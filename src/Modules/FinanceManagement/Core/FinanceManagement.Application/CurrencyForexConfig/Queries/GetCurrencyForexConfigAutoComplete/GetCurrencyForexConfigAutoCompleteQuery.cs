using FinanceManagement.Application.CurrencyForexConfig.Dto;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigAutoComplete
{
    public sealed record GetCurrencyForexConfigAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<CurrencyForexConfigLookupDto>>;
}
