using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch
{
    public class DepartmentGroupAutoCompleteDto
    {
        public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; } 
        public string? DepartmentGroupName { get; set; } 
              
    }
}