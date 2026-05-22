using MediatR;
using Contracts.Common;

namespace UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin
{
    public class CreateEntityLevelAdminCommand : IRequest<int>, IRequirePermission
    {
        public string? Email { get; set; }
        public int EntityId { get; set; }
        public int CompanyId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
