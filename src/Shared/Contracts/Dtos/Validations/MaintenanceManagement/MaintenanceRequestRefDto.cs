namespace Contracts.Dtos.Validations.MaintenanceManagement
{
    /// <summary>
    /// Minimal projection of a MaintenanceRequest used for cross-line consistency checks
    /// in the Service PO validator (e.g. confirm all linked requests share the same vendor).
    /// </summary>
    public sealed class MaintenanceRequestRefDto
    {
        public int Id { get; set; }
        public int? VendorId { get; set; }
        public int? ServiceTypeId { get; set; }
        public int? RequestStatusId { get; set; }
        public string? RequestStatusCode { get; set; }
    }
}
