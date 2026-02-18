using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuDto
    {
        public string MenuName { get; set; } = default!;
        public int ModuleId { get; set; }
        public string MenuUrl { get; set; } = default!;
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
    }
}