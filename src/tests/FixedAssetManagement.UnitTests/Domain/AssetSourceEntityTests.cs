using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetSourceEntityTests
    {
        [Fact]
        public void AssetSource_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetSource();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetSource_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetSource();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetSource_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetSource)).Should().BeTrue();
        }

        [Fact]
        public void AssetSource_Properties_ShouldBeAssignable()
        {
            var entity = new AssetSource
            {
                Id = 1,
                SourceCode = "SRC001",
                SourceName = "Test Source"
            };

            entity.Id.Should().Be(1);
            entity.SourceCode.Should().Be("SRC001");
            entity.SourceName.Should().Be("Test Source");
        }

        [Fact]
        public void AssetSource_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetSource
            {
                SourceCode = null,
                SourceName = null,
                AssetPurchase = null,
                AssetAdditionalCost = null
            };

            entity.SourceCode.Should().BeNull();
            entity.SourceName.Should().BeNull();
        }
    }
}
