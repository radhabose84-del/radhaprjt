namespace MaintenanceManagement.Application.ServiceHistory.Dto
{
    /// <summary>
    /// A single, unified service-history line for a machine / asset.
    /// Derived (read-only) from completed Work Orders, Preventive Schedule logs
    /// and Maintenance Requests — no dedicated history table.
    /// </summary>
    public class ServiceHistoryDto
    {
        /// <summary>WorkOrder | PreventiveScheduleLog | MaintenanceRequest</summary>
        public string? EventType { get; set; }

        /// <summary>Primary key of the originating row in its source table.</summary>
        public int SourceId { get; set; }

        public int MachineId { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }

        /// <summary>FixedAssetManagement asset id (cross-module, via MachineMaster.AssetId).</summary>
        public int? AssetId { get; set; }

        /// <summary>Work Order document number (null for log / request rows).</summary>
        public string? DocNo { get; set; }

        /// <summary>WO status description / log action type / request status description.</summary>
        public string? ActionOrStatus { get; set; }

        public string? Description { get; set; }

        /// <summary>When the service / event occurred (used for ordering).</summary>
        public DateTimeOffset? PerformedOn { get; set; }

        public decimal? TotalSpentHours { get; set; }
        public int? TotalManPower { get; set; }

        /// <summary>Origin of a preventive-schedule log entry (null for other sources).</summary>
        public string? Source { get; set; }

        /// <summary>Success flag for preventive-schedule log entries (null for other sources).</summary>
        public bool? IsSuccess { get; set; }

        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
    }
}
