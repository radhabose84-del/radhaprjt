using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById
{
    public class ApprovalRequestWithLinesDto
    {
         public int Id { get; set; }
        public int ModuleTransactionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public int? ApprovalRuleId { get; set; }
        public int StatusId { get; set; }
        public DateTimeOffset RequestedDate { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public string Remark { get; set; }
        public string WorkflowType { get; set; }
        public int WorkflowTypeId { get; set; }
        public string ApproverBinding { get; set; }
        public string ApproverValue { get; set; }

        public List<ApprovalRequestDto> Lines { get; set; } = new();
    }
}