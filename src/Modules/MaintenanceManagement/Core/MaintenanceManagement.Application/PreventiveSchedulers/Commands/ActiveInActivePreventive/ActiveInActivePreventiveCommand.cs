using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive
{
    public class ActiveInActivePreventiveCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public byte IsActive { get; set; }
    }
}