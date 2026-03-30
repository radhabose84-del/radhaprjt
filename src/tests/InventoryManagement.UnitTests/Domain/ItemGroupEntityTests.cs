using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemGroupEntityTests
    {
        [Fact]
        public void ItemGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemGroup)).Should().BeTrue();
        }

        [Fact]
        public void ItemGroup_Properties_ShouldBeAssignable()
        {
            var entity = new ItemGroup
            {
                Id = 1,
                ItemGroupCode = "IG001",
                ItemGroupName = "Electronics",
                UnitId = 5
            };

            entity.Id.Should().Be(1);
            entity.ItemGroupCode.Should().Be("IG001");
            entity.ItemGroupName.Should().Be("Electronics");
            entity.UnitId.Should().Be(5);
        }

        [Fact]
        public void ItemGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemGroup
            {
                ItemGroupCode = null,
                ItemGroupName = null,
                ItemCategory = null,
                ItemMasterGroup = null
            };

            entity.ItemGroupCode.Should().BeNull();
            entity.ItemGroupName.Should().BeNull();
            entity.ItemCategory.Should().BeNull();
            entity.ItemMasterGroup.Should().BeNull();
        }
    }
}
