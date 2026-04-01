using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemVariantValueEntityTests
    {
        [Fact]
        public void ItemVariantValue_Properties_ShouldBeAssignable()
        {
            var entity = new ItemVariantValue
            {
                Id = 1,
                ItemId = 10,
                VariantAttributeId = 3,
                OptionValue = "Blue",
                ParentItemId = 5
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.VariantAttributeId.Should().Be(3);
            entity.OptionValue.Should().Be("Blue");
            entity.ParentItemId.Should().Be(5);
        }

        [Fact]
        public void ItemVariantValue_DefaultOptionValue_CanBeAssigned()
        {
            var entity = new ItemVariantValue { OptionValue = "Red" };
            entity.OptionValue.Should().Be("Red");
        }
    }
}
