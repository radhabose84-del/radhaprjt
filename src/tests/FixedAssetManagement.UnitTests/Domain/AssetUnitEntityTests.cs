using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetUnitEntityTests
    {
        [Fact]
        public void AssetUnit_Properties_ShouldBeAssignable()
        {
            var entity = new AssetUnit
            {
                OldUnitId = "U100",
                UnitName = "Main Plant"
            };

            entity.OldUnitId.Should().Be("U100");
            entity.UnitName.Should().Be("Main Plant");
        }

        [Fact]
        public void AssetUnit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetUnit();

            entity.OldUnitId.Should().BeNull();
            entity.UnitName.Should().BeNull();
        }
    }
}
