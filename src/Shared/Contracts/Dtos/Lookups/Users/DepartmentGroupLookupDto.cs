using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class DepartmentGroupLookupDto
    {
        public int DepartmentGroupId { get; set; }
        public string? DepartmentGroupName { get; set; }
    }
}
