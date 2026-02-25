using MediatR;

namespace UserManagement.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}