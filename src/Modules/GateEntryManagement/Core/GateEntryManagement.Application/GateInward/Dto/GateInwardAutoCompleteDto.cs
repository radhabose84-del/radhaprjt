namespace GateEntryManagement.Application.GateInward.Dto
{
    public sealed class GateInwardAutoCompleteDto
    {
        public int Id { get; set; }
        public string? GateEntryNo { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
    }
}
