using InventoryManagement.Domain.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemSpecificationMasterEntityTests
    {
        [Fact]
        public void ItemSpecificationMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DomainEntities.ItemSpecificationMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemSpecificationMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DomainEntities.ItemSpecificationMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemSpecificationMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DomainEntities.ItemSpecificationMaster)).Should().BeTrue();
        }

        [Fact]
        public void ItemSpecificationMaster_Properties_ShouldBeAssignable()
        {
            var entity = new DomainEntities.ItemSpecificationMaster
            {
                Id = 1,
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 5
            };
            entity.Id.Should().Be(1);
            entity.SpecificationCode.Should().Be("SPEC001");
            entity.SpecificationName.Should().Be("Color");
            entity.Order.Should().Be(5);
        }

        [Fact]
        public void ItemSpecificationMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DomainEntities.ItemSpecificationMaster
            {
                SpecificationCode = null,
                SpecificationName = null
            };
            entity.SpecificationCode.Should().BeNull();
            entity.SpecificationName.Should().BeNull();
        }

        [Fact]
        public void ItemSpecificationMaster_NavigationProperties_ShouldBeInitializedEmpty()
        {
            var entity = new DomainEntities.ItemSpecificationMaster();
            entity.SpecificationValues.Should().NotBeNull().And.BeEmpty();
            entity.VariantAttributes.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ItemSpecificationMaster_NavigationProperties_ShouldBeAssignable()
        {
            var values = new List<DomainEntities.ItemSpecificationValue>
            {
                new DomainEntities.ItemSpecificationValue { Id = 10, SpecificationValue = "Red" }
            };
            var entity = new DomainEntities.ItemSpecificationMaster
            {
                SpecificationValues = values
            };
            entity.SpecificationValues.Should().HaveCount(1);
            entity.SpecificationValues.First().SpecificationValue.Should().Be("Red");
        }
    }
}
