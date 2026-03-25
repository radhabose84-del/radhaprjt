namespace GateEntryManagement.Application.VehicleMovementRecord.Dto
{
    public sealed class VehicleMovementRecordAutoCompleteDto
    {
        public int Id { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? StatusName { get; set; }
    }
}
