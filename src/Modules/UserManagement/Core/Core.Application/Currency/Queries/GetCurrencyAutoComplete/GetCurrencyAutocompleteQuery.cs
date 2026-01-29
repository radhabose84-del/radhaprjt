using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Currency.Queries.GetCurrency;
using MediatR;

namespace Core.Application.Currency.Queries.GetCurrencyAutoComplete
{
    public class GetCurrencyAutocompleteQuery : IRequest<List<CurrencyAutoCompleteDto>>
    {
        public string SearchPattern { get; set; }
    }
}