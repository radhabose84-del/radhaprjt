using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement
{
    public class RoleMenuPrivilegesDTO
    {
      public int MenuId { get; set; }
      public bool CanView { get; set; }
      public bool CanAdd { get; set; }   
      public bool CanUpdate { get; set; }
      public bool CanDelete { get; set; }
      public bool CanExport { get; set; }
      public bool CanApprove { get; set; }
    }
}