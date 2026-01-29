using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class DepartmentGroup : BaseEntity
    {
        public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }    

         public IList<Department>? Departments { get; set; }          
    }
} 