using MediatR;
using Microsoft.AspNetCore.Http;
using Contracts.Common;

namespace UserManagement.Application.UserSignature.Command.CreateUserSignature
{
    public class CreateUserSignatureCommand : IRequest<int>, IRequirePermission
    {
        public int UserId { get; set; }
        public IFormFile? File { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
