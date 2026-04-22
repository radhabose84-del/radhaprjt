using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetGrnItemEntityTests
    {
        [Fact]
        public void AssetGrnItem_Properties_ShouldBeAssignable()
        {
            var entity = new AssetGrnItem
            {
                GrnSerialNo = 42,
                ItemName = "Desktop Computer"
            };

            entity.GrnSerialNo.Should().Be(42);
            entity.ItemName.Should().Be("Desktop Computer");
        }

        [Fact]
        public void AssetGrnItem_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetGrnItem();

            entity.ItemName.Should().BeNull();
        }

        [Fact]
        public void AssetGrnItem_GrnSerialNo_DefaultShouldBeZero()
        {
            var entity = new AssetGrnItem();

            entity.GrnSerialNo.Should().Be(0);
        }
    }
}
