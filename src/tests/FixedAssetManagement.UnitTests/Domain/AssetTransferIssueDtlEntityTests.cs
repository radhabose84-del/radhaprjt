using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetTransferIssueDtlEntityTests
    {
        [Fact]
        public void AssetTransferIssueDtl_Properties_ShouldBeAssignable()
        {
            var entity = new AssetTransferIssueDtl
            {
                Id = 1,
                AssetTransferId = 10,
                AssetId = 5,
                AssetValue = 125000m
            };

            entity.Id.Should().Be(1);
            entity.AssetTransferId.Should().Be(10);
            entity.AssetId.Should().Be(5);
            entity.AssetValue.Should().Be(125000m);
        }

        [Fact]
        public void AssetTransferIssueDtl_NavigationProperties_ShouldBeAssignable()
        {
            var hdr = new AssetTransferIssueHdr();
            var assetMaster = new AssetMasterGenerals();

            var entity = new AssetTransferIssueDtl
            {
                Id = 1,
                AssetTransferId = 10,
                AssetId = 5,
                AssetValue = 50000m,
                AssetTransferIssueHdr = hdr,
                AssetMasterTransferIssue = assetMaster
            };

            entity.AssetTransferIssueHdr.Should().BeSameAs(hdr);
            entity.AssetMasterTransferIssue.Should().BeSameAs(assetMaster);
        }
    }
}
