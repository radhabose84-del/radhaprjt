using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserById
{
    public class GetMachineGroupUserByIdQuery : IRequest<MachineGroupUserDto>
    {
        public int Id { get; set; }
    }
}