using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int UserId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
