using FAM.Application.SubLocation.Queries.GetSubLocations;
using MediatR;
using Contracts.Common;

namespace FAM.Application.SubLocation.Command.CreateSubLocation
{
    public class CreateSubLocationCommand : IRequest<SubLocationDto>, IRequirePermission
    {
        public string? Code { get; set; }
        public string? SubLocationName { get; set; }
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
