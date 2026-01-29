using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveLineStatusDto
    {
        public int ApprovalRequestLineId { get; set; }
        public int ModuleLineTransactionId { get; set; }
        public int NewStatusId { get; set; }
        public byte IsApproved { get; set; }
    }
}