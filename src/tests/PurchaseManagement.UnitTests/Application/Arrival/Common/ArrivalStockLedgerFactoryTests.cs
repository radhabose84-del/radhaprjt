using PurchaseManagement.Application.Arrival.Common;

namespace PurchaseManagement.UnitTests.Application.Arrival.Common
{
    public sealed class ArrivalStockLedgerFactoryTests
    {
        private static readonly DateTimeOffset Doc = new(2026, 6, 8, 0, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Build_SavesPayloadBalesVerbatim()
        {
            var lines = new[]
            {
                new ArrivalStockLedgerFactory.LineInput(13, 4, new[]
                {
                    new ArrivalStockLedgerFactory.BaleEntry(100001, 221.5m, 900001),
                    new ArrivalStockLedgerFactory.BaleEntry(100002, 223.0m, null)   // not scanned
                })
            };

            var rows = ArrivalStockLedgerFactory.Build(Doc, lines);

            rows.Should().HaveCount(2);
            rows[0].BaleNo.Should().Be(100001);
            rows[0].BarcodeNumber.Should().Be(900001);
            rows[0].BaleWeight.Should().Be(221.5m);
            rows[1].BaleNo.Should().Be(100002);
            rows[1].BarcodeNumber.Should().BeNull();
            rows[1].BaleWeight.Should().Be(223.0m);
            rows.Should().OnlyContain(r => r.DocType == "ARV" && r.DocDate == Doc);
        }

        [Fact]
        public void Build_LineWithNoBaleEntries_ProducesNoRows()
        {
            var lines = new[]
            {
                new ArrivalStockLedgerFactory.LineInput(13, 4, null)
            };

            var rows = ArrivalStockLedgerFactory.Build(Doc, lines);

            rows.Should().BeEmpty();
        }

        [Fact]
        public void Build_NoLines_ReturnsEmpty()
        {
            var rows = ArrivalStockLedgerFactory.Build(Doc, Array.Empty<ArrivalStockLedgerFactory.LineInput>());
            rows.Should().BeEmpty();
        }
    }
}
