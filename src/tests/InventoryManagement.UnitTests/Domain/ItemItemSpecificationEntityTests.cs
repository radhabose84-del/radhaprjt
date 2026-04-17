using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemItemSpecificationEntityTests
    {
        [Fact]
        public void ItemItemSpecification_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemItemSpecification();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemItemSpecification_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemItemSpecification();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemItemSpecification_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemItemSpecification)).Should().BeTrue();
        }

        [Fact]
        public void ItemItemSpecification_Properties_ShouldBeAssignable()
        {
            var entity = new ItemItemSpecification
            {
                Id = 1,
                ItemId = 10,
                SpecificationValueId = 20
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.SpecificationValueId.Should().Be(20);
        }

        [Fact]
        public void ItemItemSpecification_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new ItemItemSpecification
            {
                ItemMaster = null,
                SpecificationValue = null
            };

            entity.ItemMaster.Should().BeNull();
            entity.SpecificationValue.Should().BeNull();
        }
    }
}
