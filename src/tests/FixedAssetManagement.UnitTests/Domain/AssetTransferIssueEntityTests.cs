using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetTransferIssueEntityTests
    {
        [Fact]
        public void AssetTransferIssue_Properties_ShouldBeAssignable()
        {
            var entity = new AssetTransferIssue
            {
                assetId = 10,
                AssetCategoryId = 5,
                AssetName = "Printer",
                UnitId = 3,
                DepartmentId = 7,
                CustodianId = 12,
                UserID = 1,
                LocationId = 4,
                SubLocationId = 8,
                assetTransferId = 20
            };

            entity.assetId.Should().Be(10);
            entity.AssetCategoryId.Should().Be(5);
            entity.AssetName.Should().Be("Printer");
            entity.UnitId.Should().Be(3);
            entity.DepartmentId.Should().Be(7);
            entity.CustodianId.Should().Be(12);
            entity.UserID.Should().Be(1);
            entity.LocationId.Should().Be(4);
            entity.SubLocationId.Should().Be(8);
            entity.assetTransferId.Should().Be(20);
        }

        [Fact]
        public void AssetTransferIssue_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetTransferIssue();

            entity.AssetName.Should().BeNull();
            entity.AssetMasterId.Should().BeNull();
        }

        [Fact]
        public void AssetTransferIssue_NavigationProperty_ShouldBeAssignable()
        {
            var assetMaster = new AssetMasterGenerals();
            var entity = new AssetTransferIssue
            {
                AssetMasterId = assetMaster
            };

            entity.AssetMasterId.Should().BeSameAs(assetMaster);
        }
    }
}
