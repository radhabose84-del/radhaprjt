using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory
{
    public class ApprovedHistoryDto
    {
        public string ApproverName { get; set; }
        public int StepOrder { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public DateTimeOffset RequestedDate { get; set; }
        public DateTimeOffset ApprovedDate { get; set; }
        public string ApproverValue { get; set; }
    }
}