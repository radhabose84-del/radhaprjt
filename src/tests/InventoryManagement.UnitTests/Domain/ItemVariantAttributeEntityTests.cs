using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemVariantAttributeEntityTests
    {
        [Fact]
        public void ItemVariantAttribute_Properties_ShouldBeAssignable()
        {
            var entity = new ItemVariantAttribute
            {
                Id = 1,
                ItemId = 10,
                SpecificationMasterId = 4,
                Order = 1
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.SpecificationMasterId.Should().Be(4);
            entity.Order.Should().Be(1);
        }

        [Fact]
        public void ItemVariantAttribute_NavigationProperties_ShouldBeAssignable()
        {
            var specMaster = new ItemSpecificationMaster { Id = 4, SpecificationName = "Color" };
            var entity = new ItemVariantAttribute
            {
                SpecificationMasterId = 4,
                SpecificationMaster = specMaster
            };

            entity.SpecificationMaster.Should().NotBeNull();
            entity.SpecificationMaster!.SpecificationName.Should().Be("Color");
        }
    }
}
