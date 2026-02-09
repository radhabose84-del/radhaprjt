using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup
{
    public class ProjectWbsLookupDto

    {
        public int Id { get; set; }
        
        public string WorkBreakdownStructureName { get; set; } = string.Empty;
    }
}