using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetDisposalEntityTests
    {
        [Fact]
        public void AssetDisposal_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetDisposal();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetDisposal_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetDisposal();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetDisposal_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetDisposal)).Should().BeTrue();
        }

        [Fact]
        public void AssetDisposal_Properties_ShouldBeAssignable()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var entity = new AssetDisposal
            {
                Id = 1,
                AssetId = 10,
                AssetPurchaseId = 5,
                DisposalDate = today,
                DisposalType = 2,
                DisposalReason = "Obsolete",
                DisposalAmount = 1000.50m
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.AssetPurchaseId.Should().Be(5);
            entity.DisposalDate.Should().Be(today);
            entity.DisposalReason.Should().Be("Obsolete");
        }

        [Fact]
        public void AssetDisposal_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetDisposal
            {
                DisposalType = null,
                DisposalReason = null,
                DisposalAmount = null
            };

            entity.DisposalType.Should().BeNull();
            entity.DisposalReason.Should().BeNull();
            entity.DisposalAmount.Should().BeNull();
        }
    }
}
