using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Menu.Queries.GetParentMenu
{
    public class ParentMenuDto
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = default!;
    }
}