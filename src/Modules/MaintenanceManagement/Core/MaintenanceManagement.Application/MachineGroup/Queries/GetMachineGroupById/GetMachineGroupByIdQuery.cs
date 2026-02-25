using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetMachineGroupByIdQuery   :  IRequest<GetMachineGroupByIdDto>
    {
        public int Id { get; set; }
    }
}