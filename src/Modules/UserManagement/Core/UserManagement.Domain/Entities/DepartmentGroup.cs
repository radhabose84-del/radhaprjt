using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class DepartmentGroup : BaseEntity
    {
        public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }    

         public IList<Department>? Departments { get; set; }          
    }
} 