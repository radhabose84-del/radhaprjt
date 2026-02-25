using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOrderItemConsuption
{
    public class WorkOrderIssueQuery : IRequest<ApiResponseDTO<List<WorkOrderIssueDto>>>
    {
        public DateTimeOffset? IssueFrom { get; set; }
        public DateTimeOffset? IssueTo { get; set; }
       
    }
}