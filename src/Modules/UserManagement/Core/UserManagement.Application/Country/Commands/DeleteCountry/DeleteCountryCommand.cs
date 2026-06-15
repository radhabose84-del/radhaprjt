using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Country.Commands.DeleteCountry
{
    public class DeleteCountryCommand :  IRequest<CountryDto>, IRequirePermission
       {
                public int Id { get; set; }                
                public PermissionType RequiredPermission => PermissionType.CanDelete;
       }
    
}
