using Core.Application.Country.Queries.GetCountries;
using MediatR;     
using Core.Application.Common.HttpResponse;

namespace Core.Application.Country.Commands.CreateCountry
{     
     public class CreateCountryCommand :  IRequest<CountryDto>
     {
          public string? CountryCode { get; set; }
          public string? CountryName { get; set; } 
     }         

}