using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOrderItemConsuption
{
    public class WorkOrderIssueQuery : IRequest<ApiResponseDTO<List<WorkOrderIssueDto>>>
    {
        public DateTimeOffset? IssueFrom { get; set; }
        public DateTimeOffset? IssueTo { get; set; }
       
    }
}