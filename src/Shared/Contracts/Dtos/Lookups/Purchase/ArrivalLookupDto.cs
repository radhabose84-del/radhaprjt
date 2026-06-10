namespace Contracts.Dtos.Lookups.Purchase
{
    /// <summary>
    /// Read-side projection of an Arrival line (ArrivalDetail + its ArrivalHeader fields)
    /// for QC inspection. Carries Purchase-owned data only — names for Supplier/Item
    /// are resolved by the consuming module via ISupplierLookup / IItemLookup
    /// (no cross-module JOINs). Shapes parallel to <see cref="GrnLookupDto"/>.
    /// </summary>
    public sealed class ArrivalLookupDto
    {
        public int ArrivalHeaderId { get; set; }
        public int ArrivalDetailId { get; set; }
        public string? ArrivalNumber { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }
        public int SupplierId { get; set; }
        public int ItemId { get; set; }
        public string? BatchNumber { get; set; }
        public decimal ReceivedQuantity { get; set; }   // mapped from ArrivalDetail.ArrivedQty
        public int? ReceivedUomId { get; set; }          // mapped from ArrivalDetail.UomId
    }
}
