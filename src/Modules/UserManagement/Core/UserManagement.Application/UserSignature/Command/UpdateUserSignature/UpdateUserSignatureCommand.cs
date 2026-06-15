using MediatR;
using Microsoft.AspNetCore.Http;
using static UserManagement.Domain.Enums.Common.Enums;
using Contracts.Common;

namespace UserManagement.Application.UserSignature.Command.UpdateUserSignature
{
    public class UpdateUserSignatureCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public IFormFile? File { get; set; }
        public Status IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
