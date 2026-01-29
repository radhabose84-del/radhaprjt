using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class PrevetiveSchedulerDetailBulkImportDto
    {
         public int PreventiveSchedulerHeaderId { get; set; }
        public int MachineId { get; set; }
        public DateOnly WorkOrderCreationStartDate { get; set; }
        public DateOnly? ActualWorkOrderDate { get; set; }
        public DateOnly MaterialReqStartDays { get; set; }
        public string? HangfireJobId { get; set; }
        public DateOnly? LastMaintenanceActivityDate { get; set; }
    }
}