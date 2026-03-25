namespace GateEntryManagement.Application.GatePass.Dto
{
    public sealed class GatePassAutoCompleteDto
    {
        public int Id { get; set; }
        public string? GatePassNo { get; set; }
        public DateOnly GatePassDate { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
    }
}
