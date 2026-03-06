using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory
{
    public class GetApprovedHistoryQuery : IRequest<List<ApprovedHistoryDto>>
    {
        public string WorkflowType { get; set; }
        public int ModuleTransactionId { get; set; }
    }
}