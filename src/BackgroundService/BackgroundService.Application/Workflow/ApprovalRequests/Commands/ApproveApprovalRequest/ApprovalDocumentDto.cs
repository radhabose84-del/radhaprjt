using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApprovalDocumentDto
    {
         public int ApprovalRequestId { get; set; }
         public string FileName { get; set; }
         public string FilePath { get; set; }
    }
}