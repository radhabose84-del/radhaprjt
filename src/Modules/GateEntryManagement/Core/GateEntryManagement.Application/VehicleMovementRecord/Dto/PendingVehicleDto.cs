namespace GateEntryManagement.Application.VehicleMovementRecord.Dto
{
    public class PendingVehicleDto
    {
        public int Id { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobileNo { get; set; }
        public string? TransporterName { get; set; }
        public DateTimeOffset GateInTime { get; set; }
        public string? StatusName { get; set; }
    }
}
