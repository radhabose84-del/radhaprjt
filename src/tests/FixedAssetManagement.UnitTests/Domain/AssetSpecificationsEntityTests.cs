using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetSpecificationsEntityTests
    {
        [Fact]
        public void AssetSpecifications_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetSpecifications();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetSpecifications_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetSpecifications();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetSpecifications_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetSpecifications)).Should().BeTrue();
        }

        [Fact]
        public void AssetSpecifications_Properties_ShouldBeAssignable()
        {
            var entity = new AssetSpecifications
            {
                Id = 1,
                AssetId = 10,
                SpecificationId = 5,
                SpecificationValue = "100kg"
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.SpecificationId.Should().Be(5);
            entity.SpecificationValue.Should().Be("100kg");
        }

        [Fact]
        public void AssetSpecifications_NullableProperties_AcceptNull()
        {
            var entity = new AssetSpecifications
            {
                SpecificationValue = null
            };

            entity.SpecificationValue.Should().BeNull();
        }
    }
}
