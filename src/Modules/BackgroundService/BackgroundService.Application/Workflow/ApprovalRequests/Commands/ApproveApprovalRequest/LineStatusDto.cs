using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class LineStatusDto
    {
        public int ModuleLineTransactionId { get; set; }
        public string Status { get; set; }
    }
}