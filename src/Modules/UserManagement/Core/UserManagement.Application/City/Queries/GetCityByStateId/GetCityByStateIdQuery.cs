using UserManagement.Application.City.Queries.GetCities;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCityByStateId
{
    public class GetCityByStateIdQuery : IRequest<List<CityDto>>
    {
        public int Id { get; set; }
    }
}