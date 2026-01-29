using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById
{
    public class ApprovalRequestDto
    {
         public int Id { get; set; }
        public int ApprovalRequestId { get; set; }
        public int ModuleLineTransactionId { get; set; }
        public int StatusId { get; set; }        
        public string? Remark { get; set; }
    }
}