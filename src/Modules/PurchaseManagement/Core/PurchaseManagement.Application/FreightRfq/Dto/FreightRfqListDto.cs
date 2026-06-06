namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>Grid row for the Freight RFQ list page.</summary>
    public class FreightRfqListDto
    {
        public int Id { get; set; }
        public string? FreightRfqNumber { get; set; }
        public DateTimeOffset RfqDate { get; set; }
        public string? RfqTypeName { get; set; }
        public string? PoNumber { get; set; }
        public string? Route { get; set; }                       // "SourceStation → DestinationStation"
        public int QuotesCount { get; set; }
        public int? ApprovedTransporterId { get; set; }
        public string? ApprovedTransporterName { get; set; }      // resolved via ITransporterLookup
        public decimal? ApprovedFreightValue { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}
