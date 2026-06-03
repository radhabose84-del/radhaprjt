using MediatR;

namespace UserManagement.Application.Station.Command.CreateStation
{
    public class CreateStationCommand : IRequest<int>
    {
        public string? Code { get; set; }
        public string? StationName { get; set; }
        public string? Description { get; set; }
    }
}
