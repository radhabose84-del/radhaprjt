using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemCategoryModuleEntityTests
    {
        [Fact]
        public void ItemCategoryModule_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemCategoryModule)).Should().BeFalse();
        }

        [Fact]
        public void ItemCategoryModule_Properties_ShouldBeAssignable()
        {
            var entity = new ItemCategoryModule
            {
                Id = 1,
                ItemCategoryId = 10,
                ModuleId = 5
            };

            entity.Id.Should().Be(1);
            entity.ItemCategoryId.Should().Be(10);
            entity.ModuleId.Should().Be(5);
        }

        [Fact]
        public void ItemCategoryModule_NavigationProperty_ShouldBeAssignable()
        {
            var category = new ItemCategory { Id = 10, ItemGroupId = 3 };
            var entity = new ItemCategoryModule
            {
                Id = 1,
                ItemCategoryId = 10,
                ItemCategory = category,
                ModuleId = 5
            };

            entity.ItemCategory.Should().NotBeNull();
            entity.ItemCategory.Id.Should().Be(10);
        }

        [Fact]
        public void ItemCategoryModule_DefaultValues_ShouldBeZero()
        {
            var entity = new ItemCategoryModule();

            entity.Id.Should().Be(0);
            entity.ItemCategoryId.Should().Be(0);
            entity.ModuleId.Should().Be(0);
        }
    }
}
