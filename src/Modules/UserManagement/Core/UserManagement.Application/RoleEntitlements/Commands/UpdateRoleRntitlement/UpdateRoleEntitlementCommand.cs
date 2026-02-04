using MediatR;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement
{
    public class UpdateRoleEntitlementCommand : IRequest<bool>
    {
        public int RoleId { get; set; }
     public IList<RoleModuleDTO>? RoleModules { get; set; }
     public IList<RoleParentDTO>? RoleParents { get; set; }
     public IList<RoleChildDTO>? RoleChildren { get; set; }
     public IList<RoleMenuPrivilegesDTO>? RoleMenuPrivileges { get; set; }
    }
}