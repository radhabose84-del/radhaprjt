namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>
    /// Values auto-filled into the Freight RFQ form when a PO Reference is selected.
    /// Only what the PO actually carries — route and bale count are entered manually.
    /// </summary>
    public class FreightRfqPoPrefillDto
    {
        public int PoReferenceId { get; set; }
        public string? PoNumber { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal TotalQuantity { get; set; }   // sum of PO line quantities
    }
}
