
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser
{
    public class UpdateMachineGroupUserCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int MachineGroupId { get; set; }
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public byte IsActive { get; set; }
    }
}