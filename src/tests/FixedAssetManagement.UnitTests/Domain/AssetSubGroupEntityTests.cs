using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetSubGroupEntityTests
    {
        [Fact]
        public void AssetSubGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetSubGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetSubGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetSubGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetSubGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetSubGroup)).Should().BeTrue();
        }

        [Fact]
        public void AssetSubGroup_Properties_ShouldBeAssignable()
        {
            var entity = new AssetSubGroup
            {
                Id = 1,
                Code = "SG001",
                SubGroupName = "Test Sub Group",
                SortOrder = 5,
                GroupId = 2,
                SubGroupPercentage = 10.5m,
                AdditionalDepreciation = 1
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("SG001");
            entity.SubGroupName.Should().Be("Test Sub Group");
            entity.SortOrder.Should().Be(5);
            entity.GroupId.Should().Be(2);
            entity.SubGroupPercentage.Should().Be(10.5m);
            entity.AdditionalDepreciation.Should().Be(1);
        }

        [Fact]
        public void AssetSubGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetSubGroup
            {
                Code = null,
                SubGroupName = null
            };

            entity.Code.Should().BeNull();
            entity.SubGroupName.Should().BeNull();
        }
    }
}
