namespace PurchaseManagement.Application.Arrival.Dto
{
    /// <summary>
    /// Approved freight pulled from the Approved Freight RFQ tied to a Raw Material PO, used to prefill
    /// the arrival's Transport &amp; Lorry section. Field names mirror the screen so the UI binds directly.
    /// The approved transporter is itself a Party, so it surfaces under both Transporter and Party.
    /// RFQ id/number are reference only (not shown on screen).
    /// </summary>
    public sealed class ApprovedFreightDto
    {
        public int? TransporterId { get; set; }        // → "Transporter *" value
        public string? Transporter { get; set; }        // → "Transporter *" display (via ITransporterLookup)
        public int? PartyId { get; set; }               // → "Party *" value (= TransporterId)
        public string? Party { get; set; }              // → "Party *" display (= Transporter)
        public decimal? FreightRate { get; set; }       // → "Freight Rate"
        public int FreightRfqId { get; set; }           // source reference
        public string? FreightRfqNumber { get; set; }   // source reference
    }
}
