using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetActivityMasterByIdQuery   :  IRequest<GetActivityMasterByIdDto>
    {
        public int Id { get; set; }
    }
}