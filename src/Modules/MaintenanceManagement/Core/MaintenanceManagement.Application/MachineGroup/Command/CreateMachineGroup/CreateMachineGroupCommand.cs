using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup
{
    public class CreateMachineGroupCommand : IRequest<int>, IRequirePermission
    {
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte PowerSource { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
