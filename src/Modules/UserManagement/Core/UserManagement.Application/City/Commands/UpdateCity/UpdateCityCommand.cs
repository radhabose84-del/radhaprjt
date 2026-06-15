using UserManagement.Application.City.Queries.GetCities;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.City.Commands.UpdateCity
{
    public class UpdateCityCommand : IRequest<CityDto>, IRequirePermission
       {
                public int Id { get; set; }
                public string? CityCode { get; set; }
                public string? CityName { get; set; }                
                public int StateId { get; set; }
                public byte IsActive { get; set; } 
                public PermissionType RequiredPermission => PermissionType.CanUpdate;
         }
  
}
