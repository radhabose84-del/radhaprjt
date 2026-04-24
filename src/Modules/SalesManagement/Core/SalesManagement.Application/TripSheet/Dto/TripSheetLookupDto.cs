namespace SalesManagement.Application.TripSheet.Dto
{
    public sealed class TripSheetLookupDto
    {
        public int Id { get; set; }
        public string? TripSheetNo { get; set; }
        public string? VehicleNo { get; set; }
        public DateOnly TripDate { get; set; }
    }
}
