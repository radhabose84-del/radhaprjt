using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Modules.Commands.UpdateModule
{
    public class UpdateModuleCommand : IRequest<bool>, IRequirePermission
    {
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    // public List<string>? Menus { get; set; }
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
