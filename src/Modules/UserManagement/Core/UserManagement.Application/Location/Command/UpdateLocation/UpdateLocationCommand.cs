using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Location.Command.UpdateLocation
{
    public class UpdateLocationCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public Status IsActive { get; set; }
    }
}
