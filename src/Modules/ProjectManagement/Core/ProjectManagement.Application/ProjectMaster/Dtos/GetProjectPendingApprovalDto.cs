using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals
{
    public class GetProjectPendingApprovalDto
    {
           public int Id { get; set;}
          
            public string ProjectCode { get; set; } = default!;
            public string ProjectName { get; set; } = default!;
            public string? ProjectDescription { get; set; }
            public int ProjectTypeId { get; set; }
            public int UnitId { get; set; }
            public int? DepartmentId { get; set; }
            public string? DepartmentName { get; set; }

            public decimal BudgetAmount { get; set; }
            public int? BudgetYearId { get; set; }
            public int? CostCenterId { get; set; }
            public int? CurrencyId { get; set; }

            public DateTimeOffset? StartDate { get; set; }
            public DateTimeOffset? EndDate { get; set; }

            public int StatusId { get; set; }
            public string? Status { get; set; }

            public int? ApproverId { get; set; }
            public string? ApproverName { get; set; }
            public int ApprovalRequestHeaderId { get; set; }
    }
}