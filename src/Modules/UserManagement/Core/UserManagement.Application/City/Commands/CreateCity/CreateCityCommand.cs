using UserManagement.Application.City.Queries.GetCities;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.City.Commands.CreateCity
{
    public class CreateCityCommand : IRequest<CityDto>, IRequirePermission
    {
        public int StateId { get; set; }
        public string? CityCode { get; set; } 
        public string? CityName { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
