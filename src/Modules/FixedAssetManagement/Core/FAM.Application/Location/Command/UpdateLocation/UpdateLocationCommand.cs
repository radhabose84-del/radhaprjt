using MediatR;
using Contracts.Common;

namespace FAM.Application.Location.Command.UpdateLocation
{
    public class UpdateLocationCommand: IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
