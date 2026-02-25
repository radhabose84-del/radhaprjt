using MediatR;

namespace UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement
{
    public class CreateRoleEntitlementCommand : IRequest<bool>
    {
        
         public int RoleId { get; set; }
     public IList<RoleModuleDTO>? RoleModules { get; set; }
     public IList<RoleParentDTO>? RoleParents { get; set; }
     public IList<RoleChildDTO>? RoleChildren { get; set; }
     public IList<RoleMenuPrivilegesDTO>? RoleMenuPrivileges { get; set; }
        

    }

}