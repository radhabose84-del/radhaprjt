using InventoryManagement.Domain.Entities.Stock;

namespace InventoryManagement.UnitTests.Domain
{
    public class SubStoreStockLedgerEntityTests
    {
        [Fact]
        public void SubStoreStockLedger_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new SubStoreStockLedger
            {
                Id = 1,
                UnitId = 2,
                DocType = "MRS",
                DocNo = 200,
                DocSlNo = 1,
                DocDate = now,
                DepartmentId = 3,
                ItemId = 10,
                UomId = 4,
                WarehouseId = 5,
                StorageTypeId = 6,
                TargetId = 7,
                ReceivedQty = 30m,
                ReceivedValue = 3000m,
                IssueQty = 5m,
                IssueValue = 500m
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(2);
            entity.DocType.Should().Be("MRS");
            entity.DocNo.Should().Be(200);
            entity.DepartmentId.Should().Be(3);
            entity.ItemId.Should().Be(10);
            entity.UomId.Should().Be(4);
            entity.WarehouseId.Should().Be(5);
            entity.StorageTypeId.Should().Be(6);
            entity.TargetId.Should().Be(7);
            entity.ReceivedQty.Should().Be(30m);
            entity.IssueQty.Should().Be(5m);
        }

        [Fact]
        public void SubStoreStockLedger_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SubStoreStockLedger
            {
                DocType = null,
                StorageTypeId = null,
                TargetId = null,
                ReceivedQty = null,
                ReceivedValue = null,
                IssueQty = null,
                IssueValue = null
            };

            entity.DocType.Should().BeNull();
            entity.StorageTypeId.Should().BeNull();
            entity.TargetId.Should().BeNull();
            entity.ReceivedQty.Should().BeNull();
            entity.IssueQty.Should().BeNull();
        }
    }
}
