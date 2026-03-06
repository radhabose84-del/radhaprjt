using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class HeaderStatusDto
    {
        public int ApprovalRequestId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string WorkflowType { get; set; }
        public string StatusCode { get; set; }
    }
}