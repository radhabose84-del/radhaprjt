using Contracts.Common;
using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;

namespace UserManagement.Application.Country.Queries.GetCountryAutoComplete
{
    public class GetCountryAutoCompleteQuery : IRequest<List<CountryAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}