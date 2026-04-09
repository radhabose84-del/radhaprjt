using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetTransferReceiptHdrEntityTests
    {
        [Fact]
        public void AssetTransferReceiptHdr_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AssetTransferReceiptHdr
            {
                Id = 1,
                AssetTransferId = 10,
                DocDate = now,
                Sdcno = "SDC001",
                GatePassNo = "GP001",
                AuthorizedBy = 5,
                AuthorizedDate = now,
                AuthorizedByName = "Admin",
                AuthorizedIP = "127.0.0.1",
                Remarks = "Test remarks"
            };

            entity.Id.Should().Be(1);
            entity.AssetTransferId.Should().Be(10);
            entity.DocDate.Should().Be(now);
            entity.Sdcno.Should().Be("SDC001");
            entity.GatePassNo.Should().Be("GP001");
            entity.AuthorizedBy.Should().Be(5);
            entity.AuthorizedByName.Should().Be("Admin");
            entity.AuthorizedIP.Should().Be("127.0.0.1");
            entity.Remarks.Should().Be("Test remarks");
        }

        [Fact]
        public void AssetTransferReceiptHdr_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetTransferReceiptHdr
            {
                Id = 1,
                AssetTransferId = 10,
                DocDate = DateTimeOffset.UtcNow,
                Sdcno = null,
                GatePassNo = null,
                AuthorizedBy = null,
                AuthorizedDate = null,
                AuthorizedByName = null,
                AuthorizedIP = null,
                Remarks = null
            };

            entity.Sdcno.Should().BeNull();
            entity.GatePassNo.Should().BeNull();
            entity.AuthorizedBy.Should().BeNull();
            entity.AuthorizedDate.Should().BeNull();
            entity.AuthorizedByName.Should().BeNull();
            entity.AuthorizedIP.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void AssetTransferReceiptHdr_CollectionProperty_ShouldBeAssignable()
        {
            var entity = new AssetTransferReceiptHdr
            {
                Id = 1,
                AssetTransferId = 10,
                DocDate = DateTimeOffset.UtcNow,
                AssetTransferReceiptDtl = new List<AssetTransferReceiptDtl>
                {
                    new AssetTransferReceiptDtl { Id = 1, AssetId = 100 }
                }
            };

            entity.AssetTransferReceiptDtl.Should().HaveCount(1);
        }
    }
}
