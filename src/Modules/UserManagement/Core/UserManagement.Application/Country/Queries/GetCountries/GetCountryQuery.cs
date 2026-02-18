using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Country.Queries.GetCountries
{   
   public class GetCountryQuery : IRequest<ApiResponseDTO<List<CountryDto>>>
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; } 
      public string? SearchTerm { get; set; }
   }
          
}