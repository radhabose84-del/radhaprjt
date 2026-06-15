using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Modules.Commands.DeleteModule
{
    public class DeleteModuleCommand : IRequest<bool>, IRequirePermission
    {
        public int ModuleId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
