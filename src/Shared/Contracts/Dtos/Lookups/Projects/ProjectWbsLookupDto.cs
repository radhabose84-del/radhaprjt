using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Projects
{
    public class ProjectWbsLookupDto
    {
        public int WbsId { get; set; }
        public int ProjectId { get; set; }
        public string? WorkBreakdownStructureName { get; set; }
    }
}
