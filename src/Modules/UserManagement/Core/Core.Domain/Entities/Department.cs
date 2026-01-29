using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Common;


namespace Core.Domain.Entities
{

    public class Department : BaseEntity
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentGroupId { get; set; }        
        public IList<UserDepartment>? UserDepartments { get; set; }
        
          [ForeignKey("DepartmentGroupId")]
        public DepartmentGroup? DepartmentGroup { get; set; }                  

    }          
}