using Core.Application.City.Queries.GetCities;
using MediatR;
using Core.Application.Common.HttpResponse;

namespace Core.Application.City.Commands.CreateCity
{     
    public class CreateCityCommand : IRequest<CityDto>
    {
        public int StateId { get; set; }
        public string? CityCode { get; set; } 
        public string? CityName { get; set; } 
    }
}