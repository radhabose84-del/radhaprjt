namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>Pending Purchase Order option for the Freight RFQ "PO Reference" dropdown.</summary>
    public class PoReferenceLookupDto
    {
        public int Id { get; set; }
        public string? PoNumber { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
    }
}
