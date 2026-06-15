using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Country.Commands.UpdateCountry
{
    public class UpdateCountryCommand : IRequest<CountryDto>, IRequirePermission
       {
              public int Id { get; set; }
              public string? CountryCode { get; set; }
              public string? CountryName { get; set; }
              public byte IsActive { get; set; } 
              public PermissionType RequiredPermission => PermissionType.CanUpdate;
         }
  
}
