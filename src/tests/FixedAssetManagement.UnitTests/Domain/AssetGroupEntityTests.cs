using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetGroupEntityTests
    {
        [Fact]
        public void AssetGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetGroup)).Should().BeTrue();
        }

        [Fact]
        public void AssetGroup_Properties_ShouldBeAssignable()
        {
            var entity = new AssetGroup
            {
                Id = 1,
                Code = "AG001",
                GroupName = "Test Group",
                SortOrder = 5,
                GroupPercentage = 10.5m
            };
            entity.Id.Should().Be(1);
            entity.Code.Should().Be("AG001");
            entity.GroupName.Should().Be("Test Group");
            entity.SortOrder.Should().Be(5);
            entity.GroupPercentage.Should().Be(10.5m);
        }
    }
}
