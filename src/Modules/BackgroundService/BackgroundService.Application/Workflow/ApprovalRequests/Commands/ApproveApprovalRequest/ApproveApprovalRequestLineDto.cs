using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestLineDto
    {
        public int ApprovalRequestLineId { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public int ModuleLineTransactionId { get; set; }
        // public decimal ApprovedQuantity { get; set; }
        // public string ApproverBinding { get; set; }
        // public string ApproverValue { get; set; }
        // public int StatusId { get; set; }
        public string Remark { get; set; }
        public byte IsApproved { get; set; }
    }
}