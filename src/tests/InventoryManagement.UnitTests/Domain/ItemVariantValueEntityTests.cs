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
                SpecificationValueId = 7,
                ParentItemId = 5
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.VariantAttributeId.Should().Be(3);
            entity.SpecificationValueId.Should().Be(7);
            entity.ParentItemId.Should().Be(5);
        }

        [Fact]
        public void ItemVariantValue_NavigationProperties_ShouldBeAssignable()
        {
            var specValue = new ItemSpecificationValue { Id = 7, SpecificationValue = "Blue" };
            var entity = new ItemVariantValue
            {
                SpecificationValueId = 7,
                SpecificationValue = specValue
            };

            entity.SpecificationValue.Should().NotBeNull();
            entity.SpecificationValue!.SpecificationValue.Should().Be("Blue");
        }
    }
}
