using InventoryManagement.Domain.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemSpecificationValueEntityTests
    {
        [Fact]
        public void ItemSpecificationValue_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DomainEntities.ItemSpecificationValue();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemSpecificationValue_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DomainEntities.ItemSpecificationValue();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemSpecificationValue_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DomainEntities.ItemSpecificationValue)).Should().BeTrue();
        }

        [Fact]
        public void ItemSpecificationValue_Properties_ShouldBeAssignable()
        {
            var entity = new DomainEntities.ItemSpecificationValue
            {
                Id = 1,
                SpecificationMasterId = 5,
                SpecificationValue = "Red"
            };
            entity.Id.Should().Be(1);
            entity.SpecificationMasterId.Should().Be(5);
            entity.SpecificationValue.Should().Be("Red");
        }

        [Fact]
        public void ItemSpecificationValue_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DomainEntities.ItemSpecificationValue
            {
                SpecificationValue = null,
                SpecificationMaster = null
            };
            entity.SpecificationValue.Should().BeNull();
            entity.SpecificationMaster.Should().BeNull();
        }

        [Fact]
        public void ItemSpecificationValue_NavigationProperties_ShouldBeInitializedEmpty()
        {
            var entity = new DomainEntities.ItemSpecificationValue();
            entity.VariantValues.Should().NotBeNull().And.BeEmpty();
            entity.ItemSpecifications.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ItemSpecificationValue_SpecificationMasterNavigation_ShouldBeAssignable()
        {
            var master = new DomainEntities.ItemSpecificationMaster { Id = 5, SpecificationName = "Color" };
            var entity = new DomainEntities.ItemSpecificationValue
            {
                SpecificationMasterId = 5,
                SpecificationMaster = master
            };
            entity.SpecificationMaster.Should().NotBeNull();
            entity.SpecificationMaster!.SpecificationName.Should().Be("Color");
        }
    }
}
