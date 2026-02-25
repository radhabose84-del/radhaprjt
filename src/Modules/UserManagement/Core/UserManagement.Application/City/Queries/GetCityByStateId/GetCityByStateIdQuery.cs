using UserManagement.Application.City.Queries.GetCities;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCityByStateId
{
    public class GetCityByStateIdQuery : IRequest<List<CityDto>>
    {
        public int Id { get; set; }
    }
}