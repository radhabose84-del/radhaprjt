using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCityAutoComplete
{
    public class GetCityAutoCompleteQuery : IRequest<List<CityAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}