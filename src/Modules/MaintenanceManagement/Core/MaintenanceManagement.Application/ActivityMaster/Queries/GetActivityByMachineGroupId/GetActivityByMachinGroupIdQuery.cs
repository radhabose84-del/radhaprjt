using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId
{
    public class GetActivityByMachinGroupIdQuery :IRequest<List<GetActivityByMachineGroupDto>>
    {

     public int MachineGroupId { get; set; }
        
    }
}