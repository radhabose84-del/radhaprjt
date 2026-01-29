using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Reports.WorkOrderItemConsuption
{
    public class WorkOrderIssueDto
    {
        public int ReqId { get; set; }
        public DateTimeOffset ReqDate { get; set; }
        public int WorkOrderId { get; set; }
        public DateTimeOffset WorkOrderDate { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? WorkOrderDocNo { get; set; }
        public string? Description { get; set; }
        public string? MachineName { get; set; }
        public string? MachineCode { get; set; }
        public string? IssueNo { get; set; }
        public DateTimeOffset Issuedate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal IssueQty { get; set; }
        public decimal IssueValue { get; set; }
        public string? OldUnitCode { get; set; }
        public decimal Rate { get; set; }
        public DateTime? LastChangedDate { get; set; }
        public int ProductionDepartmentId { get; set; }
        public string? ProductionDepartmentName { get; set; }
        public int MaintenanceDepartmentId { get; set; }
        public string? MaintenanceDepartmentName { get; set; }

    }
}