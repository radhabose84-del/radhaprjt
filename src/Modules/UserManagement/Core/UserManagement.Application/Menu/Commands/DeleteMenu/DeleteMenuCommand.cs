using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
