using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetGrnEntityTests
    {
        [Fact]
        public void AssetGrn_Properties_ShouldBeAssignable()
        {
            var entity = new AssetGrn
            {
                OldUnitId = "U001",
                GrnNo = "GRN-2025-001"
            };

            entity.OldUnitId.Should().Be("U001");
            entity.GrnNo.Should().Be("GRN-2025-001");
        }

        [Fact]
        public void AssetGrn_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetGrn();

            entity.OldUnitId.Should().BeNull();
            entity.GrnNo.Should().BeNull();
        }
    }
}
