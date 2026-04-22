using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class StockLedgerEntityTests
    {
        [Fact]
        public void StockLedger_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(StockLedger)).Should().BeFalse();
        }

        [Fact]
        public void StockLedger_Properties_ShouldBeAssignable()
        {
            var date = new DateTime(2026, 3, 10);
            var entity = new StockLedger
            {
                Id = 1,
                OldUnitCode = "U01",
                TransactionType = "Receipt",
                DocNo = 2001,
                DocSNo = 1,
                DocDate = date,
                ItemCode = "ITM001",
                ItemName = "Bearing",
                UOM = "NOS",
                ReceivedQty = 50.0m,
                ReceivedValue = 7500.0m,
                IssueQty = 10.0m,
                IssueValue = 1500.0m,
                CreatedDate = date
            };
            entity.Id.Should().Be(1);
            entity.OldUnitCode.Should().Be("U01");
            entity.TransactionType.Should().Be("Receipt");
            entity.DocNo.Should().Be(2001);
            entity.DocSNo.Should().Be(1);
            entity.DocDate.Should().Be(date);
            entity.ItemCode.Should().Be("ITM001");
            entity.ItemName.Should().Be("Bearing");
            entity.UOM.Should().Be("NOS");
            entity.ReceivedQty.Should().Be(50.0m);
            entity.ReceivedValue.Should().Be(7500.0m);
            entity.IssueQty.Should().Be(10.0m);
            entity.IssueValue.Should().Be(1500.0m);
            entity.CreatedDate.Should().Be(date);
        }

        [Fact]
        public void StockLedger_NullableProperties_ShouldAcceptNull()
        {
            var entity = new StockLedger
            {
                OldUnitCode = null,
                TransactionType = null,
                ItemCode = null,
                ItemName = null,
                UOM = null
            };
            entity.OldUnitCode.Should().BeNull();
            entity.TransactionType.Should().BeNull();
            entity.ItemCode.Should().BeNull();
            entity.ItemName.Should().BeNull();
            entity.UOM.Should().BeNull();
        }
    }
}
