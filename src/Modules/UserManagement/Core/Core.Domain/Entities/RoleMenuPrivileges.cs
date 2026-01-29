using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class RoleMenuPrivileges
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
        public bool CanApprove { get; set; }
        public UserRole? UserRole { get; set; }
        public Menu? Menu { get; set; }
        
    }
}