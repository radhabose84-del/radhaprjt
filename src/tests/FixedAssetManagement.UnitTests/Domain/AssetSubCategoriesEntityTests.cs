using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetSubCategoriesEntityTests
    {
        [Fact]
        public void AssetSubCategories_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetSubCategories();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetSubCategories_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetSubCategories();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetSubCategories_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetSubCategories)).Should().BeTrue();
        }

        [Fact]
        public void AssetSubCategories_Properties_ShouldBeAssignable()
        {
            var entity = new AssetSubCategories
            {
                Id = 1,
                Code = "SC001",
                SubCategoryName = "Test SubCategory",
                Description = "Test description",
                SortOrder = 3,
                AssetCategoriesId = 2
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("SC001");
            entity.SubCategoryName.Should().Be("Test SubCategory");
            entity.Description.Should().Be("Test description");
            entity.SortOrder.Should().Be(3);
            entity.AssetCategoriesId.Should().Be(2);
        }

        [Fact]
        public void AssetSubCategories_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetSubCategories
            {
                Code = null,
                SubCategoryName = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.SubCategoryName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
