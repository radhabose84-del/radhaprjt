using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetTransferReceiptDtlEntityTests
    {
        [Fact]
        public void AssetTransferReceiptDtl_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AssetTransferReceiptDtl
            {
                Id = 1,
                AssetReceiptId = 10,
                AssetId = 100,
                LocationId = 5,
                SubLocationId = 3,
                UserID = "USR001",
                UserName = "TestUser",
                AckStatus = 1,
                AckDate = now
            };

            entity.Id.Should().Be(1);
            entity.AssetReceiptId.Should().Be(10);
            entity.AssetId.Should().Be(100);
            entity.LocationId.Should().Be(5);
            entity.SubLocationId.Should().Be(3);
            entity.UserID.Should().Be("USR001");
            entity.UserName.Should().Be("TestUser");
            entity.AckStatus.Should().Be(1);
            entity.AckDate.Should().Be(now);
        }

        [Fact]
        public void AssetTransferReceiptDtl_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetTransferReceiptDtl
            {
                Id = 1,
                AssetReceiptId = 10,
                AssetId = 100,
                LocationId = null,
                SubLocationId = null,
                UserID = null,
                UserName = null,
                AckDate = null
            };

            entity.LocationId.Should().BeNull();
            entity.SubLocationId.Should().BeNull();
            entity.UserID.Should().BeNull();
            entity.UserName.Should().BeNull();
            entity.AckDate.Should().BeNull();
        }
    }
}
