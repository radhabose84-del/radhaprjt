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
                VariantBasedOn = 2,
                AttributeGroupId = 3,
                AttributeId = 4,
                Order = 1
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.VariantBasedOn.Should().Be(2);
            entity.AttributeGroupId.Should().Be(3);
            entity.AttributeId.Should().Be(4);
            entity.Order.Should().Be(1);
        }

        [Fact]
        public void ItemVariantAttribute_NullableAttributeGroupId_ShouldAcceptNull()
        {
            var entity = new ItemVariantAttribute { AttributeGroupId = null };
            entity.AttributeGroupId.Should().BeNull();
        }
    }
}
