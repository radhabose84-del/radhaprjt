namespace Contracts.Dtos.Lookups.Gate
{
    /// <summary>
    /// Cross-module snapshot of a <c>Gate.VehicleMovementRecord</c> (VMR) row, used by
    /// PurchaseManagement (Arrival) to display VMR details for an arrival's <c>VmrId</c>.
    /// </summary>
    public sealed class VehicleMovementRecordLookupDto
    {
        public int Id { get; set; }
        public string? VehicleMovementId { get; set; }   // system VMR number
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobileNo { get; set; }
        public int? TransporterId { get; set; }
        public string? ReferenceDocNo { get; set; }
        public DateTimeOffset? GateInTime { get; set; }
        public DateTimeOffset? GateOutTime { get; set; }
        public int? StatusId { get; set; }
    }
}
