using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemUOMEntityTests
    {
        [Fact]
        public void ItemUOM_Properties_ShouldBeAssignable()
        {
            var entity = new ItemUOM
            {
                Id = 1,
                ItemId = 10,
                ConversionUOMId = 3,
                ConversionRate = 1000m
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.ConversionUOMId.Should().Be(3);
            entity.ConversionRate.Should().Be(1000m);
        }

        [Fact]
        public void ItemUOM_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemUOM
            {
                ConversionUOMId = null,
                ConversionRate = null
            };

            entity.ConversionUOMId.Should().BeNull();
            entity.ConversionRate.Should().BeNull();
        }
    }
}
