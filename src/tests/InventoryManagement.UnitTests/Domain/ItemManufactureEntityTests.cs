using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemManufactureEntityTests
    {
        [Fact]
        public void ItemManufacture_Properties_ShouldBeAssignable()
        {
            var entity = new ItemManufacture
            {
                Id = 1,
                ItemId = 10,
                UnitId = 2,
                ManufacturingTypeId = 3
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.UnitId.Should().Be(2);
            entity.ManufacturingTypeId.Should().Be(3);
        }

        [Fact]
        public void ItemManufacture_DefaultValues_ShouldBeZero()
        {
            var entity = new ItemManufacture();
            entity.Id.Should().Be(0);
            entity.ItemId.Should().Be(0);
            entity.UnitId.Should().Be(0);
            entity.ManufacturingTypeId.Should().Be(0);
        }
    }
}
