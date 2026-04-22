using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.UnitTests.Domain
{
    public class SubStoreStockLedgerEntityTests
    {
        [Fact]
        public void SubStoreStockLedger_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SubStoreStockLedger)).Should().BeFalse();
        }

        [Fact]
        public void SubStoreStockLedger_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new SubStoreStockLedger
            {
                Id = 1,
                UnitId = 10,
                DocType = "ISSUE",
                DocNo = 100,
                DocSlNo = 1,
                DocDate = now,
                DepartmentId = 3,
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
            entity.DocType.Should().Be("ISSUE");
            entity.DocNo.Should().Be(100);
            entity.DepartmentId.Should().Be(3);
            entity.ItemId.Should().Be(50);
            entity.WarehouseId.Should().Be(5);
            entity.ReceivedQty.Should().Be(100m);
            entity.IssueQty.Should().Be(20m);
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
            entity.IssueValue.Should().BeNull();
        }
    }
}
