using Core.Application.City.Queries.GetCities;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.City.Queries.GetCityByStateId
{
    public class GetCityByStateIdQuery : IRequest<List<CityDto>>
    {
        public int Id { get; set; }
    }
}