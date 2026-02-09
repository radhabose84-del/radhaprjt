using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals
{
    public class ProjectPendingApprovalDto
    {
         public int Id { get; set; }
    public string ProjectCode { get; set; } = default!;
    public string ProjectName { get; set; } = default!;
    public decimal BudgetAmount { get; set; }
    public int UnitId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectTypeId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = "Pending";
    }
}