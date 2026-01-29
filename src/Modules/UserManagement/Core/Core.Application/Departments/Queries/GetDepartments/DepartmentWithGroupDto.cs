using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Departments.Queries.GetDepartments
{
    public class DepartmentWithGroupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int DepartmentGroupId { get; set; }
        public string? DepartmentGroupName { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}