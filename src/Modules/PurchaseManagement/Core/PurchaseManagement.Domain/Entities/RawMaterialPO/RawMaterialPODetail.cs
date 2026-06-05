using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.RawMaterialPO
{
    /// <summary>
    /// Raw-material PO line item with full GST breakup. Table: [Purchase].[RawMaterialPODetail].
    /// Computed money fields (ItemValue, GST values, TotalGST, NetValue) are set in the command
    /// handler — never trusted from the client.
    /// </summary>
    public class RawMaterialPODetail : BaseEntity
    {
        public int POHeaderId { get; set; }
        public RawMaterialPOHeader RawMaterialPOMaster { get; set; } = default!;

        // ── Cross-module FK columns — NO DB constraint, populated via lookup on read ──
        public int ItemId { get; set; }   // Inventory ItemMaster (IItemLookup) — cotton
        public int HsnId { get; set; }    // Inventory HSN (IHSNLookup) — source of GST rates

        public decimal Quantity { get; set; }    // Bales (to convert)
        public decimal? Weight { get; set; }      // Kgs
        public decimal Rate { get; set; }         // Per candy

        // ── Computed in command handler ──
        public decimal ItemValue { get; set; }            // = Rate * Quantity
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public decimal? CGSTValue { get; set; }
        public decimal? SGSTValue { get; set; }
        public decimal? IGSTValue { get; set; }
        public decimal TotalGST { get; set; }             // = CGSTValue + SGSTValue + IGSTValue
        public decimal NetValue { get; set; }             // = ItemValue + TotalGST
    }
}
