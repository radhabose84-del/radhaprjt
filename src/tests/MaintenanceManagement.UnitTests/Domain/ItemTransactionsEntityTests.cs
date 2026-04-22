using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ItemTransactionsEntityTests
    {
        [Fact]
        public void ItemTransactions_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ItemTransactions)).Should().BeFalse();
        }

        [Fact]
        public void ItemTransactions_Properties_ShouldBeAssignable()
        {
            var date = new DateTime(2026, 1, 15);
            var entity = new ItemTransactions
            {
                Id = 1,
                OldUnitCode = "U01",
                TC = 100,
                TransactionType = "Issue",
                DocNo = 5001,
                DocSNo = 1,
                DocDate = date,
                ItemCode = "ITM001",
                ItemName = "Bearing",
                UOM = "NOS",
                Quantity = 10.5m,
                Rate = 150.75m,
                Value = 1582.88m,
                CategoryDescription = "Spares",
                GroupName = "Mechanical",
                LifeType = "Months",
                LifeSpan = 12,
                DepartmentName = "Maintenance",
                CreatedDate = date
            };
            entity.Id.Should().Be(1);
            entity.OldUnitCode.Should().Be("U01");
            entity.TC.Should().Be(100);
            entity.TransactionType.Should().Be("Issue");
            entity.DocNo.Should().Be(5001);
            entity.DocSNo.Should().Be(1);
            entity.DocDate.Should().Be(date);
            entity.ItemCode.Should().Be("ITM001");
            entity.ItemName.Should().Be("Bearing");
            entity.UOM.Should().Be("NOS");
            entity.Quantity.Should().Be(10.5m);
            entity.Rate.Should().Be(150.75m);
            entity.Value.Should().Be(1582.88m);
            entity.CategoryDescription.Should().Be("Spares");
            entity.GroupName.Should().Be("Mechanical");
            entity.LifeType.Should().Be("Months");
            entity.LifeSpan.Should().Be(12);
            entity.DepartmentName.Should().Be("Maintenance");
            entity.CreatedDate.Should().Be(date);
        }

        [Fact]
        public void ItemTransactions_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemTransactions
            {
                OldUnitCode = null,
                TransactionType = null,
                ItemCode = null,
                ItemName = null,
                UOM = null,
                CategoryDescription = null,
                GroupName = null,
                LifeType = null,
                DepartmentName = null
            };
            entity.OldUnitCode.Should().BeNull();
            entity.TransactionType.Should().BeNull();
            entity.ItemCode.Should().BeNull();
            entity.ItemName.Should().BeNull();
            entity.UOM.Should().BeNull();
            entity.CategoryDescription.Should().BeNull();
            entity.GroupName.Should().BeNull();
            entity.LifeType.Should().BeNull();
            entity.DepartmentName.Should().BeNull();
        }
    }
}
