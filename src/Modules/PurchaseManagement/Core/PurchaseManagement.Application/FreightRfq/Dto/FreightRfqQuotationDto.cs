namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>A single transporter quotation row returned with the Freight RFQ.</summary>
    public class FreightRfqQuotationDto
    {
        public int Id { get; set; }
        public int FreightRfqHeaderId { get; set; }
        public int TransporterId { get; set; }
        public string? TransporterName { get; set; }   // resolved via ITransporterLookup
        public int RateBasisId { get; set; }
        public string? RateBasisName { get; set; }       // resolved via Purchase.MiscMaster
        public decimal QuotedRate { get; set; }
        public int? NoOfVehicles { get; set; }
        public decimal FreightValue { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOverride { get; set; }
        public string? Remarks { get; set; }
    }
}
