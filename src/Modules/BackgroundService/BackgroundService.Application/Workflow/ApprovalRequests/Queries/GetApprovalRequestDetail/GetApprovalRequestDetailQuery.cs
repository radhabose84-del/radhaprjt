using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestDetail
{
    public class GetApprovalRequestDetailQuery : IRequest<List<ApprovalRequestDetailDto>>
    {
        public int ModuleTransactionId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;

        /// <summary>
        /// true = Pending only (default), false = all statuses.
        /// </summary>
        public bool Pending { get; set; } = true;
    }
}
