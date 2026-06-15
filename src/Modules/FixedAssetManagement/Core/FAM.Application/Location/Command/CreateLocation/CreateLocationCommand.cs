using FAM.Application.Location.Queries.GetLocations;
using MediatR;
using Contracts.Common;

namespace FAM.Application.Location.Command.CreateLocation
{
    public class CreateLocationCommand : IRequest<LocationDto>, IRequirePermission
    {
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
