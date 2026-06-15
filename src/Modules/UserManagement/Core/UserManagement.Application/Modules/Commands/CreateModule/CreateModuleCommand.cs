using MediatR;
using Contracts.Common;


namespace UserManagement.Application.Modules.Commands.CreateModule
{
    public class CreateModuleCommand  : IRequest<int>, IRequirePermission
    {
    public string? ModuleName { get; set; }
    // public List<string>? Menus { get; set; }
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
