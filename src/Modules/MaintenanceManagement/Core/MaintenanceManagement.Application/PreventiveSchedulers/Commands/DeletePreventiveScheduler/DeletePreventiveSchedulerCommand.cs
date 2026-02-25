using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler
{
    public class DeletePreventiveSchedulerCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}