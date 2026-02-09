using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Projects
{
    public class ProjectLookupDto
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectCode { get; set; }
    }
}
