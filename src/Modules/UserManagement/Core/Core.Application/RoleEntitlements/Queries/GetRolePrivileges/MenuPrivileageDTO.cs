using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.RoleEntitlements.Commands.GetRolePrivileges
{
    public class MenuPrivileageDTO
    {
        public int Id { get; set; }
        public byte CanAdd { get; set; }
        public byte CanView { get; set; }
        public byte CanUpdate { get; set; }
        public byte CanDelete { get; set; }
        public byte CanExport { get; set; }
        public byte CanApprove { get; set; }
    }
}