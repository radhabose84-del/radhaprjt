using InventoryManagement.Domain.Entities.Stock;

namespace InventoryManagement.UnitTests.Domain
{
    public class StockLedgerEntityTests
    {
        [Fact]
        public void StockLedger_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new StockLedger
            {
                Id = 1,
                UnitId = 2,
                DocType = "GR",
                DocNo = 100,
                DocSlNo = 1,
                DocDate = now,
                ItemId = 10,
                UomId = 3,
                WarehouseId = 5,
                StorageTypeId = 6,
                TargetId = 7,
                ReceivedQty = 50m,
                ReceivedValue = 5000m,
                IssueQty = 10m,
                IssueValue = 1000m
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(2);
            entity.DocType.Should().Be("GR");
            entity.DocNo.Should().Be(100);
            entity.DocSlNo.Should().Be(1);
            entity.DocDate.Should().Be(now);
            entity.ItemId.Should().Be(10);
            entity.UomId.Should().Be(3);
            entity.WarehouseId.Should().Be(5);
            entity.StorageTypeId.Should().Be(6);
            entity.TargetId.Should().Be(7);
            entity.ReceivedQty.Should().Be(50m);
            entity.ReceivedValue.Should().Be(5000m);
            entity.IssueQty.Should().Be(10m);
            entity.IssueValue.Should().Be(1000m);
        }

        [Fact]
        public void StockLedger_NullableProperties_ShouldAcceptNull()
        {
            var entity = new StockLedger
            {
                DocType = null,
                ReceivedQty = null,
                ReceivedValue = null,
                IssueQty = null,
                IssueValue = null
            };

            entity.DocType.Should().BeNull();
            entity.ReceivedQty.Should().BeNull();
            entity.ReceivedValue.Should().BeNull();
            entity.IssueQty.Should().BeNull();
            entity.IssueValue.Should().BeNull();
        }
    }
}
