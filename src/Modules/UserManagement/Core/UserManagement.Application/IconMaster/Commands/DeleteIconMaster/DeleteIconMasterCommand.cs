using MediatR;

namespace UserManagement.Application.IconMaster.Commands.DeleteIconMaster
{
    public class DeleteIconMasterCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
