using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetWarrantiesEntityTests
    {
        [Fact]
        public void AssetWarranties_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetWarranties();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetWarranties_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetWarranties();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetWarranties_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetWarranties)).Should().BeTrue();
        }

        [Fact]
        public void AssetWarranties_Properties_ShouldBeAssignable()
        {
            var entity = new AssetWarranties
            {
                Id = 1,
                AssetId = 10,
                WarrantyType = 1,
                Description = "Standard Warranty"
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.WarrantyType.Should().Be(1);
            entity.Description.Should().Be("Standard Warranty");
        }

        [Fact]
        public void AssetWarranties_NullableProperties_AcceptNull()
        {
            var entity = new AssetWarranties
            {
                Description = null,
                WarrantyType = null,
                StartDate = null,
                EndDate = null,
                Period = null
            };

            entity.Description.Should().BeNull();
            entity.WarrantyType.Should().BeNull();
            entity.StartDate.Should().BeNull();
            entity.EndDate.Should().BeNull();
            entity.Period.Should().BeNull();
        }
    }
}
