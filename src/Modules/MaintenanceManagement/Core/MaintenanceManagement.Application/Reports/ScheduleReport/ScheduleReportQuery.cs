using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.ScheduleReport
{
    public class ScheduleReportQuery : IRequest<ApiResponseDTO<List<ScheduleReportDto>>>
    {
        public DateTime? FromDueDate { get; set; }
        public DateTime? ToDueDate { get; set; }
    }
}