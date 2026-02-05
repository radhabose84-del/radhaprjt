using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class DepartmentLookupDto
    {
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ShortName { get; set; }
        public int Departmentgroupid { get; set; }
    }
}