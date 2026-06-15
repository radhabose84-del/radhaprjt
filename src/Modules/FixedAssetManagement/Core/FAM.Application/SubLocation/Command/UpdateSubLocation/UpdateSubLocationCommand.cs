using MediatR;
using Contracts.Common;

namespace FAM.Application.Location.Command.UpdateSubLocation
{
    public class UpdateSubLocationCommand: IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? SubLocationName { get; set; }
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
