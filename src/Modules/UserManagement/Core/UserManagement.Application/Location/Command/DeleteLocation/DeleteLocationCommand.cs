using MediatR;

namespace UserManagement.Application.Location.Command.DeleteLocation
{
    public class DeleteLocationCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
