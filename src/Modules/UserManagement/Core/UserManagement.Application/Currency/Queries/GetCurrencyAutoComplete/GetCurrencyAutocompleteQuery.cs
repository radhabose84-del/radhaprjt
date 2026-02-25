using UserManagement.Application.Currency.Queries.GetCurrency;
using MediatR;

namespace UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete
{
    public class GetCurrencyAutocompleteQuery : IRequest<List<CurrencyAutoCompleteDto>>
    {
        public string SearchPattern { get; set; } = default!;
    }
}