using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Menu.Queries.GetMenu
{
    public class MenuDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string MenuName { get; set; }
        public string MenuIcon { get; set; }
        public string MenuUrl { get; set; }
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public int SortOrder { get; set; }
        public string CreatedAt { get; set; }
        public string? Type { get; set; }
    }
}