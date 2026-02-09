using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals
{
    public class GetProjectPendingApprovalQuery   : IRequest<(List<GetProjectPendingApprovalDto> Items, int TotalCount)>
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

        public int? ProjectId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ProjectTypeId { get; set; }
        public int? BudgetYearId { get; set; }
    }
}