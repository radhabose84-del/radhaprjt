using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemInventoryEntityTests
    {
        [Fact]
        public void ItemInventory_Properties_ShouldBeAssignable()
        {
            var entity = new ItemInventory
            {
                Id = 1,
                ItemId = 10,
                Weight = 2.5m,
                WeightUomId = 3,
                DefaultMaterialRequestTypeId = 4,
                ValuationMethodId = 5,
                ShelfLife = 365,
                UpperTolerance = 0.05m,
                LowerTolerance = 0.03m,
                BatchNumberSeries = "BN-####",
                SerialNumberSeries = "SN-####",
                ReorderLevel = 100,
                ReorderQty = 50,
                RequestTypeId = 2,
                SafetyStock = 20,
                AllowNegativeStock = false,
                BatchManagement = true,
                ApplyBatchNumber = true
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.Weight.Should().Be(2.5m);
            entity.WeightUomId.Should().Be(3);
            entity.ShelfLife.Should().Be(365);
            entity.BatchNumberSeries.Should().Be("BN-####");
            entity.ReorderLevel.Should().Be(100);
            entity.SafetyStock.Should().Be(20);
            entity.AllowNegativeStock.Should().BeFalse();
            entity.BatchManagement.Should().BeTrue();
            entity.ApplyBatchNumber.Should().BeTrue();
        }

        [Fact]
        public void ItemInventory_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemInventory
            {
                Weight = null,
                WeightUomId = null,
                DefaultMaterialRequestTypeId = null,
                ValuationMethodId = null,
                ShelfLife = null,
                UpperTolerance = null,
                LowerTolerance = null,
                BatchNumberSeries = null,
                SerialNumberSeries = null,
                ReorderLevel = null,
                ReorderQty = null,
                RequestTypeId = null,
                SafetyStock = null
            };

            entity.Weight.Should().BeNull();
            entity.WeightUomId.Should().BeNull();
            entity.ShelfLife.Should().BeNull();
            entity.BatchNumberSeries.Should().BeNull();
            entity.ReorderLevel.Should().BeNull();
        }

        [Fact]
        public void ItemInventory_DefaultBooleans_ShouldBeFalse()
        {
            var entity = new ItemInventory();
            entity.AllowNegativeStock.Should().BeFalse();
            entity.BatchManagement.Should().BeFalse();
            entity.ApplyBatchNumber.Should().BeFalse();
        }
    }
}
