using System.ComponentModel.DataAnnotations.Schema;
using UserManagement.Domain.Common;


namespace UserManagement.Domain.Entities
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