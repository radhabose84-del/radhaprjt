using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup
{
    public class UpdateMachineGroupCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
        public byte PowerSource { get; set; } 
        
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
