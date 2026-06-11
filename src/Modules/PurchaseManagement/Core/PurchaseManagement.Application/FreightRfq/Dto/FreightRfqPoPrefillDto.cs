namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>
    /// Values auto-filled into the Freight RFQ form when a Raw Material PO is selected.
    /// Supplier + source route come from the PO's OCR; quantities are summed from the PO details.
    /// Destination is entered by the user.
    /// </summary>
    public class FreightRfqPoPrefillDto
    {
        public int PoReferenceId { get; set; }
        public string? PoNumber { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SourceLocation { get; set; }   // OCR LocationId -> name
        public string? SourceStation { get; set; }     // OCR StationId -> name
        public decimal TotalQuantity { get; set; }     // MT = sum(detail Weight) / 1000
        public int TotalBaleCount { get; set; }         // sum(detail Quantity)
    }
}
