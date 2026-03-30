using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class SpecificationMastersEntityTests
    {
        [Fact]
        public void SpecificationMasters_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SpecificationMasters();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SpecificationMasters_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SpecificationMasters();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SpecificationMasters_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SpecificationMasters)).Should().BeTrue();
        }

        [Fact]
        public void SpecificationMasters_Properties_ShouldBeAssignable()
        {
            var entity = new SpecificationMasters
            {
                Id = 1,
                SpecificationName = "TestSpec",
                AssetGroupId = 2,
                ISDefault = 1
            };

            entity.Id.Should().Be(1);
            entity.SpecificationName.Should().Be("TestSpec");
            entity.AssetGroupId.Should().Be(2);
            entity.ISDefault.Should().Be(1);
        }

        [Fact]
        public void SpecificationMasters_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SpecificationMasters
            {
                SpecificationName = null,
                AssetSpecification = null
            };

            entity.SpecificationName.Should().BeNull();
            entity.AssetSpecification.Should().BeNull();
        }

        [Fact]
        public void SpecificationMasters_ISDefault_DefaultShouldBeZero()
        {
            var entity = new SpecificationMasters();
            entity.ISDefault.Should().Be(0);
        }
    }
}
