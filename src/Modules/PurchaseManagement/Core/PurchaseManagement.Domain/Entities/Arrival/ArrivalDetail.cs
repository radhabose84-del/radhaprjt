namespace PurchaseManagement.Domain.Entities.Arrival
{
    /// <summary>
    /// Arrival line item — one per variety/item. Carries the consolidated bale range for that line.
    /// Table: [Purchase].[ArrivalDetail].
    /// NOTE: Does NOT extend BaseEntity — no audit / soft-delete fields (per spec design decision).
    /// </summary>
    public class ArrivalDetail
    {
        public int Id { get; set; }

        // ── Same-module FK (ArrivalHeader) — DB constraint ──
        public int ArrivalHeaderId { get; set; }
        public ArrivalHeader ArrivalHeader { get; set; } = default!;

        // ── Cross-module FK columns — NO DB constraint, populated via lookup on read ──
        public int ItemId { get; set; }       // Inventory ItemMaster (IItemLookup) — variety
        public int HsnId { get; set; }         // Inventory HSN (IHSNLookup)
        public int PackTypeId { get; set; }    // Production PackType (IPackTypeLookup)
        public int MixCodeId { get; set; }     // MixCodeMaster — lookup/master TBD (see Gate 2 note)
        public int UomId { get; set; }         // Inventory UOM (IUOMLookup)

        public decimal Rate { get; set; }

        // ── Quantities ──
        public decimal OrderedQty { get; set; }     // From PO
        public decimal ArrivedQty { get; set; }
        public decimal CancelledQty { get; set; }
        public decimal BalanceQty { get; set; }      // computed Ordered − Arrived − Cancelled

        // ── Consolidated bale range (per line) ──
        public string BatchNumber { get; set; } = default!;
        public long BaleNumberFrom { get; set; }
        public long BaleNumberTo { get; set; }
        public int TotalBaleCount { get; set; }
    }
}
