using PurchaseManagement.Domain.Entities.Arrival;

namespace PurchaseManagement.Application.Arrival.Common
{
    /// <summary>
    /// Builds the StockLedgerRaw rows for an arrival. A line with NO bale entries is Consolidated: its
    /// [LineBaleFrom..LineBaleTo] range is expanded and each bale gets an even split of the arrival
    /// NetWeight across all consolidated bales (BarcodeNumber null). A line WITH bale entries is
    /// Individual: each entry is saved verbatim (bale number + weight + optional scanned barcode).
    /// UnitId + LotNo are set later by the command repository. DocDate = arrival's ArrivalDate.
    /// </summary>
    public static class ArrivalStockLedgerFactory
    {
        public sealed record BaleEntry(long BaleNumber, decimal BaleWeight, int? BaleCaptureMethodId, long? BarcodeNumber);

        public sealed record LineInput(
            int ItemId,
            int UomId,
            long LineBaleFrom,
            long LineBaleTo,
            IReadOnlyList<BaleEntry>? Bales);

        public static List<StockLedgerRaw> Build(DateTimeOffset docDate, decimal netWeight, IReadOnlyList<LineInput> lines)
        {
            var rows = new List<StockLedgerRaw>();
            if (lines == null || lines.Count == 0)
                return rows;

            static bool IsConsolidated(LineInput l) => l.Bales is not { Count: > 0 };

            // Even split — consolidated bales share NetWeight / (total consolidated bales).
            var consolidatedBales = lines
                .Where(l => IsConsolidated(l) && l.LineBaleTo >= l.LineBaleFrom)
                .Sum(l => l.LineBaleTo - l.LineBaleFrom + 1);
            var consolidatedWeight = consolidatedBales > 0 ? netWeight / consolidatedBales : 0m;

            foreach (var line in lines)
            {
                if (IsConsolidated(line))
                {
                    if (line.LineBaleTo < line.LineBaleFrom)
                        continue;

                    for (var bale = line.LineBaleFrom; bale <= line.LineBaleTo; bale++)
                    {
                        rows.Add(new StockLedgerRaw
                        {
                            DocDate = docDate,
                            BaleNo = bale,
                            BarcodeNumber = null,
                            BaleWeight = consolidatedWeight,
                            BaleCaptureMethodId = null,   // consolidated
                            ItemId = line.ItemId,
                            UomId = line.UomId,
                            DocType = "ARV"
                        });
                    }
                }
                else
                {
                    // Individual — each captured bale saved verbatim.
                    foreach (var entry in line.Bales!)
                    {
                        rows.Add(new StockLedgerRaw
                        {
                            DocDate = docDate,
                            BaleNo = entry.BaleNumber,
                            BarcodeNumber = entry.BarcodeNumber,
                            BaleWeight = entry.BaleWeight,
                            BaleCaptureMethodId = entry.BaleCaptureMethodId,
                            ItemId = line.ItemId,
                            UomId = line.UomId,
                            DocType = "ARV"
                        });
                    }
                }
            }

            return rows;
        }
    }
}
