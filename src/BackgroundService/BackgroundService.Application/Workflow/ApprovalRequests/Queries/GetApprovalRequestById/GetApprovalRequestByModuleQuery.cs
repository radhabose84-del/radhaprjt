using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.Common.HttpResponse;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById
{
    public class GetApprovalRequestByModuleQuery : IRequest<List<ApprovalRequestWithLinesDto>>
    {
        public int ModuleTransactionId { get; set; }
        public int WorkflowTypeId { get; set; }
        
    }
}