using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;

namespace PurchaseManagement.UnitTests.Domain
{
    public class StockLedgerEntityTests
    {
        [Fact]
        public void StockLedger_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(StockLedger)).Should().BeFalse();
        }

        [Fact]
        public void StockLedger_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new StockLedger
            {
                Id = 1,
                UnitId = 10,
                DocType = "GRN",
                DocNo = 100,
                DocSlNo = 1,
                DocDate = now,
                ItemId = 50,
                UomId = 3,
                WarehouseId = 5,
                StorageTypeId = 2,
                TargetId = 4,
                ReceivedQty = 100m,
                ReceivedValue = 5000m,
                IssueQty = 20m,
                IssueValue = 1000m
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.DocType.Should().Be("GRN");
            entity.DocNo.Should().Be(100);
            entity.DocSlNo.Should().Be(1);
            entity.DocDate.Should().Be(now);
            entity.ItemId.Should().Be(50);
            entity.WarehouseId.Should().Be(5);
            entity.ReceivedQty.Should().Be(100m);
            entity.ReceivedValue.Should().Be(5000m);
            entity.IssueQty.Should().Be(20m);
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
