using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Currency.Queries.GetCurrency;
using MediatR;

namespace UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete
{
    public class GetCurrencyAutocompleteQuery : IRequest<List<CurrencyAutoCompleteDto>>
    {
        public string SearchPattern { get; set; } = default!;
    }
}