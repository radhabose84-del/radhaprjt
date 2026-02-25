using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId
{
    public class GetMachineDepartmentbyIdQuery : IRequest<MachineDepartmentGroupDto>
    {
        public int MachineGroupId { get; set; }
    }
}