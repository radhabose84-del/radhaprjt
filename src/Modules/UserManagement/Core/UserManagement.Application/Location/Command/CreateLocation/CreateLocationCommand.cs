using MediatR;

namespace UserManagement.Application.Location.Command.CreateLocation
{
    public class CreateLocationCommand : IRequest<int>
    {
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
    }
}
