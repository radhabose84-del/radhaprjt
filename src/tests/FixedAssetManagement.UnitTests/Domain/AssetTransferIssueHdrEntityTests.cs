using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetTransferIssueHdrEntityTests
    {
        [Fact]
        public void AssetTransferIssueHdr_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AssetTransferIssueHdr
            {
                Id = 1,
                FromUnitId = 1,
                ToUnitId = 2,
                FromDepartmentId = 3,
                ToDepartmentId = 4,
                FromCustodianId = 10,
                ToCustodianId = 20,
                TransferType = 1,
                DocDate = now,
                AckStatus = 0,
                Status = "Pending"
            };

            entity.Id.Should().Be(1);
            entity.FromUnitId.Should().Be(1);
            entity.ToUnitId.Should().Be(2);
            entity.TransferType.Should().Be(1);
            entity.Status.Should().Be("Pending");
        }

        [Fact]
        public void AssetTransferIssueHdr_DoesNotExtendBaseEntity()
        {
            typeof(FAM.Domain.Common.BaseEntity).IsAssignableFrom(typeof(AssetTransferIssueHdr))
                .Should().BeFalse();
        }

        [Fact]
        public void AssetTransferIssueHdr_NullableProperties_AcceptNull()
        {
            var entity = new AssetTransferIssueHdr
            {
                TransferType = null,
                FromCustodianName = null,
                ToCustodianName = null,
                Status = null,
                GatePassNo = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedDate = null
            };

            entity.TransferType.Should().BeNull();
            entity.Status.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
        }
    }
}
