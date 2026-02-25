using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;

namespace UserManagement.Application.Country.Commands.CreateCountry
{
    public class CreateCountryCommand :  IRequest<CountryDto>
     {
          public string? CountryCode { get; set; }
          public string? CountryName { get; set; } 
     }         

}