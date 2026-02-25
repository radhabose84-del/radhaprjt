namespace MaintenanceManagement.Application.Reports.MaterialPlanningReport
{
    public class MaterialPlanningReportDto
    {
        public string MachineName { get; set; } = default!;
        public string MaintenanceCategory { get; set; } = default!;
        public string ActivityName { get; set; } = default!;
        public string ActivityType { get; set; } = default!;
        public string PlannedMaintenanceDate { get; set; } = default!;
        public string MaterialCode { get; set; } = default!;
        public string MaterialDescription { get; set; } = default!;
        public string UOM { get; set; } = default!;
        public int CurrentStock { get; set; }
        public int RequiredQty { get; set; }
        public int Shortfall_Excess { get; set; }
        public int ProductionDepartmentId { get; set; }
        public string ProductionDepartment { get; set; } = default!;
        public string MachineCode { get; set; } = default!;
    }
}