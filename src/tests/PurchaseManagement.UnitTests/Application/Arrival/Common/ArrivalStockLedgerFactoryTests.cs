using PurchaseManagement.Application.Arrival.Common;

namespace PurchaseManagement.UnitTests.Application.Arrival.Common
{
    public sealed class ArrivalStockLedgerFactoryTests
    {
        private static readonly DateTimeOffset Doc = new(2026, 6, 8, 0, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Build_Consolidated_NoEntries_ExpandsLineRange_WithEvenSplitWeight()
        {
            var lines = new[]
            {
                new ArrivalStockLedgerFactory.LineInput(13, 4, 100001, 100005, null)
            };

            var rows = ArrivalStockLedgerFactory.Build(Doc, netWeight: 20000m, lines);

            rows.Should().HaveCount(5);
            rows[0].BaleNo.Should().Be(100001);          // BaleNo holds the bale number
            rows[^1].BaleNo.Should().Be(100005);
            rows.Should().OnlyContain(r => r.BarcodeNumber == null);   // consolidated → no scanned barcode
            rows.Should().OnlyContain(r => r.BaleCaptureMethodId == null);   // consolidated → no method
            rows.Should().OnlyContain(r => r.BaleWeight == 4000m);     // 20000 / 5
            rows.Should().OnlyContain(r => r.DocType == "ARV" && r.DocDate == Doc);
        }

        [Fact]
        public void Build_Individual_SavesPayloadVerbatim()
        {
            var lines = new[]
            {
                new ArrivalStockLedgerFactory.LineInput(13, 4, 100001, 100002, new[]
                {
                    new ArrivalStockLedgerFactory.BaleEntry(100001, 221.5m, 1312, 900001),
                    new ArrivalStockLedgerFactory.BaleEntry(100002, 223.0m, 1311, null)   // manually-keyed individual bale
                })
            };

            var rows = ArrivalStockLedgerFactory.Build(Doc, netWeight: 20000m, lines);

            rows.Should().HaveCount(2);
            rows[0].BaleNo.Should().Be(100001);
            rows[0].BarcodeNumber.Should().Be(900001);
            rows[0].BaleWeight.Should().Be(221.5m);
            rows[0].BaleCaptureMethodId.Should().Be(1312);
            rows[1].BaleNo.Should().Be(100002);
            rows[1].BarcodeNumber.Should().BeNull();
            rows[1].BaleWeight.Should().Be(223.0m);
            rows[1].BaleCaptureMethodId.Should().Be(1311);
        }

        [Fact]
        public void Build_NoLines_ReturnsEmpty()
        {
            var rows = ArrivalStockLedgerFactory.Build(Doc, 20000m, Array.Empty<ArrivalStockLedgerFactory.LineInput>());
            rows.Should().BeEmpty();
        }
    }
}
