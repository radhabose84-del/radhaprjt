using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class DepartmentLookupDto
    {
        public int DepartmentId { get; set; }
        public string? DeptName { get; set; }
    }
}