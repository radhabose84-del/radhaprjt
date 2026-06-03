using MediatR;

namespace UserManagement.Application.Station.Command.DeleteStation
{
    public class DeleteStationCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
