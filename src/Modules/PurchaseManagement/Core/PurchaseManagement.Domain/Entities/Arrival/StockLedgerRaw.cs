namespace PurchaseManagement.Domain.Entities.Arrival
{
    /// <summary>
    /// Raw-material lot/bale stock ledger — one row per bale generated from an arrival's bale range.
    /// Maps internal sequence (BaleNo) to the scanned/allocated barcode (BarcodeNumber).
    /// Table: [Purchase].[StockLedgerRaw].
    /// NOTE: Does NOT extend BaseEntity — no audit / soft-delete fields (per spec design decision).
    /// </summary>
    public class StockLedgerRaw
    {
        public int Id { get; set; }

        public int UnitId { get; set; }

        // Document date — copied from the arrival's ArrivalDate.
        public DateTimeOffset DocDate { get; set; }

        // Lot reference = ArrivalHeader Id (logical link, no DB FK).
        public int LotNo { get; set; }

        public long BaleNo { get; set; }            // bale number (from the line's bale range / per bale)
        public long? BarcodeNumber { get; set; }    // scanned barcode — null for consolidated/manual capture
        public decimal BaleWeight { get; set; }     // per-bale weight

        // MiscMaster (manual entry / barcode scan) — set for Individual bales only; null for Consolidated.
        public int? BaleCaptureMethodId { get; set; }

        // ── Cross-module FK columns — NO DB constraint, populated via lookup on read ──
        public int ItemId { get; set; }           // Inventory ItemMaster (IItemLookup)
        public int UomId { get; set; }            // Inventory UOM (IUOMLookup)

        // Document type: "ARV" (arrival inward, default) / "ISS" (issue).
        public string DocType { get; set; } = "ARV";
    }
}
