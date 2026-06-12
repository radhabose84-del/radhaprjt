using PurchaseManagement.Domain.Entities.Arrival;

namespace PurchaseManagement.Application.Arrival.Common
{
    /// <summary>
    /// Builds the StockLedgerRaw rows for an arrival directly from the payload's per-bale entries.
    /// Each BaleDetails entry is saved verbatim (bale number + weight + capture method + optional
    /// scanned barcode) — no range expansion or weight auto-calculation. A line with no bale entries
    /// produces no StockLedgerRaw rows. UnitId + LotNo are set later by the command repository.
    /// DocDate = arrival's ArrivalDate.
    /// </summary>
    public static class ArrivalStockLedgerFactory
    {
        public sealed record BaleEntry(long BaleNumber, decimal BaleWeight, long? BarcodeNumber);

        public sealed record LineInput(
            int ItemId,
            int UomId,
            IReadOnlyList<BaleEntry>? Bales);

        public static List<StockLedgerRaw> Build(DateTimeOffset docDate, IReadOnlyList<LineInput> lines)
        {
            var rows = new List<StockLedgerRaw>();
            if (lines == null || lines.Count == 0)
                return rows;

            foreach (var line in lines)
            {
                if (line.Bales is not { Count: > 0 })
                    continue;

                // Save each captured bale verbatim from the payload.
                foreach (var entry in line.Bales)
                {
                    rows.Add(new StockLedgerRaw
                    {
                        DocDate = docDate,
                        BaleNo = entry.BaleNumber,
                        BarcodeNumber = entry.BarcodeNumber,
                        BaleWeight = entry.BaleWeight,
                        ItemId = line.ItemId,
                        UomId = line.UomId,
                        DocType = "ARV"
                    });
                }
            }

            return rows;
        }
    }
}
