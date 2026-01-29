using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement;

namespace Core.Application.RoleEntitlements.Queries.GetRoleEntitlementById
{
    public class GetByIdRoleEntitlementDTO
    {
        public int RoleId { get; set; }
     public IList<RoleModuleDTO>? RoleModules { get; set; }
     public IList<RoleParentDTO>? RoleParents { get; set; }
     public IList<RoleChildDTO>? RoleChildren { get; set; }
     public IList<RoleMenuPrivilegesDTO>? RoleMenuPrivileges { get; set; }
        
    }
}