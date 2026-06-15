using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserLogin.Commands.UnlockUser
{
    public class UnlockUserCommand : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public string? userName { get; set; }
    }
}