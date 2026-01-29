using Core.Application.Common.HttpResponse;
using Core.Application.Country.Queries.GetCountries;
using MediatR;

namespace Core.Application.Country.Commands.UpdateCountry
{
       public class UpdateCountryCommand : IRequest<CountryDto>
       {
              public int Id { get; set; }
              public string? CountryCode { get; set; }
              public string? CountryName { get; set; }
              public byte IsActive { get; set; } 
         }
  
}