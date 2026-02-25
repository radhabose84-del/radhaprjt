using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MaterialPlanningReport
{
    public class MaterialPlanningReportQuery : IRequest<ApiResponseDTO<List<MaterialPlanningReportDto>>>
    {
        public DateTime? FromDueDate { get; set; }
        public DateTime? ToDueDate { get; set; }
    }
}