using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Modules.Queries.GetModules
{
    public class ModuleByIdDto
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public string? IsDeleted { get; set; }

        public List<string>? Menus { get; set; }
    }
}