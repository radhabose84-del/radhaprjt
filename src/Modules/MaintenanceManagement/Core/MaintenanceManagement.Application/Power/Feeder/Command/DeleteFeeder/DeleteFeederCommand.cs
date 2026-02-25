using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder
{
    public class DeleteFeederCommand : IRequest<bool>
    {
         public int Id { get; set; }
    }
}