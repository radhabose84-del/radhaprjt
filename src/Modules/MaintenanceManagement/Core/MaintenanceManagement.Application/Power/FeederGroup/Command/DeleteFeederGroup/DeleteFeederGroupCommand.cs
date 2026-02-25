using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup
{
    public class DeleteFeederGroupCommand  : IRequest<bool>
    {
        
         public int Id { get; set; }
    }
}