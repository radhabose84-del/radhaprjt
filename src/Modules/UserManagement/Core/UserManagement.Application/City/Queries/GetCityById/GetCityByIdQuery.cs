using UserManagement.Application.City.Queries.GetCities;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCityById
{
    public class GetCityByIdQuery : IRequest<CityDto>
    {
        public int Id { get; set; }
    }
}