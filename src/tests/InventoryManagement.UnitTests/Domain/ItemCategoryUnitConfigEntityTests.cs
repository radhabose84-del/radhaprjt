using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemCategoryUnitConfigEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemCategoryUnitConfig();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemCategoryUnitConfig();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemCategoryUnitConfig)).Should().BeTrue();
        }

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ItemCategoryUnitConfig
            {
                Id = 1,
                ItemCategoryId = 10,
                UnitId = 5,
                UOMId = 2,
                MaxSampleQuantity = 12.5m
            };

            entity.Id.Should().Be(1);
            entity.ItemCategoryId.Should().Be(10);
            entity.UnitId.Should().Be(5);
            entity.UOMId.Should().Be(2);
            entity.MaxSampleQuantity.Should().Be(12.5m);
        }

        [Fact]
        public void NavigationProperties_AcceptNull()
        {
            var entity = new ItemCategoryUnitConfig
            {
                ItemCategory = null,
                UOM = null
            };

            entity.ItemCategory.Should().BeNull();
            entity.UOM.Should().BeNull();
        }
    }
}
