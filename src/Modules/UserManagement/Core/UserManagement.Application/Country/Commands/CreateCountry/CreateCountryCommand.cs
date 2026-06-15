using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Country.Commands.CreateCountry
{
    public class CreateCountryCommand :  IRequest<CountryDto>, IRequirePermission
     {
          public string? CountryCode { get; set; }
          public string? CountryName { get; set; } 
          public PermissionType RequiredPermission => PermissionType.CanAdd;
     }         

}
