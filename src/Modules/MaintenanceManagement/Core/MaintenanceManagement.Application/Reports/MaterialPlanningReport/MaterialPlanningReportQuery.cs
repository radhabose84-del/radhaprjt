using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MaterialPlanningReport
{
    public class MaterialPlanningReportQuery : IRequest<ApiResponseDTO<List<MaterialPlanningReportDto>>>
    {
        public DateTime? FromDueDate { get; set; }
        public DateTime? ToDueDate { get; set; }
    }
}