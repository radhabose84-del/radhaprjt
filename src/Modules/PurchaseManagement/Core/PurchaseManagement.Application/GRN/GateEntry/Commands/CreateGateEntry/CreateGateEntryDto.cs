namespace PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry
{
    public class CreateGateEntryDto
    {
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string? DriverName { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public string? ImagePath { get; set; }
        public string? Remarks { get; set; }
        public int ReceivingTypeId { get; set; }
        public List<GateEntryDetailDto>? GateEntryDetails { get; set; }
        public class GateEntryDetailDto
        {
            public int PoMethodId { get; set; }
            public int PoCategoryId { get; set; }
            public int PoId { get; set; }
            public DateTimeOffset PoDate { get; set; }
            public string PoCreatedBy { get; set; } = string.Empty;
            public string? GSTNumber { get; set; }
            public string? ContactDetails { get; set; }

        } 

    }
}