using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Modules.Queries.GetModules
{
    public class ModuleDto
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public string? IsDeleted { get; set; }

        
    }
}