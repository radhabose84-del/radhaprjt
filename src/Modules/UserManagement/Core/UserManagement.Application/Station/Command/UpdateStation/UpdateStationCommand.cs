using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Station.Command.UpdateStation
{
    public class UpdateStationCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? StationName { get; set; }
        public string? Description { get; set; }
        public Status IsActive { get; set; }
    }
}
