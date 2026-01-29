using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Reports.MaterialPlanningReport
{
    public class MaterialPlanningReportDto
    {
        public string MachineName { get; set; }
        public string MaintenanceCategory { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string PlannedMaintenanceDate { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDescription { get; set; }
        public string UOM { get; set; }
        public int CurrentStock { get; set; }
        public int RequiredQty { get; set; }
        public int Shortfall_Excess { get; set; }
        public int ProductionDepartmentId { get; set; }
        public string ProductionDepartment { get; set; }
        public string MachineCode { get; set; }
    }
}