using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.WorkOderCheckListReport
{
    public class WorkOderCheckListReportQuery : IRequest<ApiResponseDTO<List<WorkOderCheckListReportDto>>>
    {
        public DateTimeOffset? WorkOrderFromDate { get; set; }
        public DateTimeOffset? WorkOrderToDate { get; set; }
        public int UnitId { get; set; }
        public int?  MachineGroupId { get; set; }
        public int? MachineId { get; set; }
        public int? ActivityId { get; set; }

    }
}