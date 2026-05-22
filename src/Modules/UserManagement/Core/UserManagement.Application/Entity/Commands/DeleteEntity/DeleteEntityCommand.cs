using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Entity.Commands.DeleteEntity
{
    public class DeleteEntityCommand : IRequest<int>, IRequirePermission
    {
        public int EntityId { get; set; }
    
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
