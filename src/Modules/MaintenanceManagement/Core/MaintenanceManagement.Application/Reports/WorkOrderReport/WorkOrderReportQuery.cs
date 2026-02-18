using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOrderReport
{
    public class WorkOrderReportQuery : IRequest<ApiResponseDTO<List<WorkOrderReportDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int RequestTypeId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
 