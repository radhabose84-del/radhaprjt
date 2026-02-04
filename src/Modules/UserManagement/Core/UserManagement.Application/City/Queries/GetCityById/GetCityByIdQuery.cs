using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCityById
{
    public class GetCityByIdQuery : IRequest<CityDto>
    {
        public int Id { get; set; }
    }
}