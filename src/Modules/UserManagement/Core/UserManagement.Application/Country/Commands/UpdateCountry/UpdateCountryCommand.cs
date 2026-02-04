using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;

namespace UserManagement.Application.Country.Commands.UpdateCountry
{
       public class UpdateCountryCommand : IRequest<CountryDto>
       {
              public int Id { get; set; }
              public string? CountryCode { get; set; }
              public string? CountryName { get; set; }
              public byte IsActive { get; set; } 
         }
  
}