using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetCategoriesEntityTests
    {
        [Fact]
        public void AssetCategories_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetCategories();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetCategories_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetCategories();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetCategories_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetCategories)).Should().BeTrue();
        }

        [Fact]
        public void AssetCategories_Properties_ShouldBeAssignable()
        {
            var entity = new AssetCategories
            {
                Id = 1,
                Code = "TESTC",
                CategoryName = "Test Category",
                Description = "Test description",
                SortOrder = 5,
                AssetGroupId = 2
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("TESTC");
            entity.CategoryName.Should().Be("Test Category");
            entity.Description.Should().Be("Test description");
            entity.SortOrder.Should().Be(5);
            entity.AssetGroupId.Should().Be(2);
        }

        [Fact]
        public void AssetCategories_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetCategories
            {
                Code = null,
                CategoryName = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.CategoryName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
