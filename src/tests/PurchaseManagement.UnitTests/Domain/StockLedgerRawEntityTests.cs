using PurchaseManagement.Domain.Entities.Arrival;

namespace PurchaseManagement.UnitTests.Domain
{
    public class StockLedgerRawEntityTests
    {
        [Fact]
        public void StockLedgerRaw_DefaultDocType_ShouldBeArv()
        {
            new StockLedgerRaw().DocType.Should().Be("ARV");
        }

        [Fact]
        public void StockLedgerRaw_DoesNotInheritBaseEntity()
        {
            // Design decision: StockLedgerRaw has no audit / soft-delete fields.
            typeof(PurchaseManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(StockLedgerRaw)).Should().BeFalse();
        }

        [Fact]
        public void StockLedgerRaw_Properties_ShouldBeAssignable()
        {
            var entity = new StockLedgerRaw
            {
                Id = 1,
                UnitId = 1,
                LotNo = 5,
                BaleNo = 1,
                BarcodeNumber = 100001,
                BaleWeight = 222.222m,
                ItemId = 2,
                UomId = 3,
                DocType = "ISS"
            };

            entity.LotNo.Should().Be(5);
            entity.BarcodeNumber.Should().Be(100001);
            entity.BaleWeight.Should().Be(222.222m);
            entity.DocType.Should().Be("ISS");
        }
    }
}
