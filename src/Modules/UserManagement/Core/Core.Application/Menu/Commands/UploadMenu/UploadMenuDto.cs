using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuDto
    {
        public string MenuName { get; set; }
        public int ModuleId { get; set; }
        public string MenuUrl { get; set; }
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
    }
}