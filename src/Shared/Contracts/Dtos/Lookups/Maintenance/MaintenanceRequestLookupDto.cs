namespace Contracts.Dtos.Lookups.Maintenance
{
    /// <summary>
    /// Lookup DTO for External Maintenance Requests used when linking from a Service PO.
    /// Carries all fields the UI auto-populates on the PO when the user picks a request.
    /// </summary>
    public class MaintenanceRequestLookupDto
    {
        public int Id { get; set; }

        // Display string for the UI dropdown — RequestNo is synthesized from Id since
        // the underlying table has no separate document number column.
        public string RequestNo { get; set; } = default!;

        // Auto-population fields
        public int MachineId { get; set; }
        public string? MachineName { get; set; }
        public int MaintenanceDepartmentId { get; set; }
        public int MaintenanceTypeId { get; set; }
        public string? MaintenanceTypeName { get; set; }
        public int? ServiceTypeId { get; set; }
        public string? ServiceTypeName { get; set; }
        public int? VendorId { get; set; }
        public string? VendorName { get; set; }
        public decimal? EstimatedServiceCost { get; set; }
        public decimal ConvertedToPoAmount { get; set; }
        public string? Remarks { get; set; }

        // Status fields — included so callers can render & make policy decisions
        public int? RequestStatusId { get; set; }
        public string? RequestStatusCode { get; set; }
    }
}
