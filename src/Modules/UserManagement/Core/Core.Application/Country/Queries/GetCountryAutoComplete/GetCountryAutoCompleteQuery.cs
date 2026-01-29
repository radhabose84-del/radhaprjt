using Core.Application.Common.HttpResponse;
using Core.Application.Country.Queries.GetCountries;
using MediatR;

namespace Core.Application.Country.Queries.GetCountryAutoComplete
{
    public class GetCountryAutoCompleteQuery : IRequest<List<CountryAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}