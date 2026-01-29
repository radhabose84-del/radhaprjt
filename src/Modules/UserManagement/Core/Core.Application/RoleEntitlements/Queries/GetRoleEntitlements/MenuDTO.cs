using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.RoleEntitlements.Queries.GetRoleEntitlements
{
    public class MenuDTO
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public IList<MenuDTO>? ChildMenu { get; set; }
    }
}