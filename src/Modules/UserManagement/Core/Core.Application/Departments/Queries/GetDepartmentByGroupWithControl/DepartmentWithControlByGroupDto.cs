using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Departments.Queries.GetDepartmentByGroupWithControl
{
    public class DepartmentWithControlByGroupDto
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int DepartmentGroupId { get; set; }
        public string? DepartmentGroupName { get; set; }
    }
}