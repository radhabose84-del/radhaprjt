namespace Contracts.Dtos.Lookups.Purchase
{
    /// <summary>
    /// Read-side projection of a single GRN line (GrnDetail + its GrnHeader fields)
    /// for QC inspection. Carries Purchase-owned data only — names for Supplier/Item
    /// are resolved by the consuming module via ISupplierLookup / IItemLookup
    /// (no cross-module JOINs).
    /// </summary>
    public sealed class GrnLookupDto
    {
        public int GrnHeaderId { get; set; }
        public int GrnDetailId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset GrnDate { get; set; }
        public int SupplierId { get; set; }
        public string? InvoiceNo { get; set; }
        public int ItemId { get; set; }
        public string? BatchNumber { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public int? ReceivedUomId { get; set; }
    }
}
