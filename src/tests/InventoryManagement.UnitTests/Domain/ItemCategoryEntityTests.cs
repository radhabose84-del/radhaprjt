using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemCategoryEntityTests
    {
        [Fact]
        public void ItemCategory_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemCategory();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemCategory_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemCategory();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemCategory_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemCategory)).Should().BeTrue();
        }

        [Fact]
        public void ItemCategory_Properties_ShouldBeAssignable()
        {
            var entity = new ItemCategory
            {
                Id = 1,
                ItemCategoryName = "Electronics",
                ItemGroupId = 2,
                IsGroup = 1,
                IsBudgetApplicable = 0
            };
            entity.Id.Should().Be(1);
            entity.ItemCategoryName.Should().Be("Electronics");
            entity.ItemGroupId.Should().Be(2);
            entity.IsGroup.Should().Be(1);
        }

        [Fact]
        public void ItemCategory_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemCategory
            {
                ItemCategoryName = null,
                ParentCategoryId = null,
                IsGroup = null,
                IsBudgetApplicable = null,
                RootCategoryId = null,
                DeptId = null
            };
            entity.ItemCategoryName.Should().BeNull();
            entity.ParentCategoryId.Should().BeNull();
            entity.RootCategoryId.Should().BeNull();
        }

        [Fact]
        public void ItemCategory_ChildCategories_DefaultsToEmptyList()
        {
            var entity = new ItemCategory();
            entity.ChildCategories.Should().NotBeNull();
            entity.ChildCategories.Should().BeEmpty();
        }
    }
}
