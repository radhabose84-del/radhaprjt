using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class RoleEntitlement : BaseEntity
    {
    public int Id { get; set; }
    public int UserRoleId { get; set; }
    public UserRole? UserRole { get; set; }
    // public int ModuleId { get; set; }
    // public Modules? Module { get; set; }
    public int MenuId { get; set; }
    public Menu? Menu { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
    public bool CanApprove { get; set; }
    }
}