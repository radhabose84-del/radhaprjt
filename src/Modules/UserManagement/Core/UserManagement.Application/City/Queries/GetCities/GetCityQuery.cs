using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.City.Queries.GetCities
{   
   public class GetCityQuery : IRequest<ApiResponseDTO<List<CityDto>>>
   {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
   }
}