using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserSignature.Command.DeleteUserSignature
{
    public class DeleteUserSignatureCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
