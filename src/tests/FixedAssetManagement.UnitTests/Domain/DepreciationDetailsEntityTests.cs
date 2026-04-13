using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class DepreciationDetailsEntityTests
    {
        [Fact]
        public void DepreciationDetails_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DepreciationDetails();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DepreciationDetails_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DepreciationDetails();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DepreciationDetails_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DepreciationDetails)).Should().BeTrue();
        }

        [Fact]
        public void DepreciationDetails_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new DepreciationDetails
            {
                Id = 1,
                CompanyId = 10,
                UnitId = 20,
                Finyear = 2026,
                StartDate = now,
                EndDate = now.AddYears(1),
                DepreciationType = 1,
                AssetId = 5,
                AssetGroupId = 3,
                AssetValue = 100000m,
                CapitalizationDate = now,
                ResidualValue = 5000m,
                ExpiryDate = now.AddYears(10),
                UsefulLifeDays = 3650,
                DaysOpening = 365,
                DaysUsed = 30,
                OpeningValue = 95000m,
                DepreciationValue = 1000m,
                ClosingValue = 94000m,
                NetValue = 94000m,
                IsLocked = 1,
                DepreciationPeriod = 12,
                DisposedDate = now.AddYears(5),
                DisposalAmount = 50000m
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(10);
            entity.UnitId.Should().Be(20);
            entity.Finyear.Should().Be(2026);
            entity.DepreciationType.Should().Be(1);
            entity.AssetId.Should().Be(5);
            entity.AssetGroupId.Should().Be(3);
            entity.AssetValue.Should().Be(100000m);
            entity.ResidualValue.Should().Be(5000m);
            entity.UsefulLifeDays.Should().Be(3650);
            entity.DaysOpening.Should().Be(365);
            entity.DaysUsed.Should().Be(30);
            entity.OpeningValue.Should().Be(95000m);
            entity.DepreciationValue.Should().Be(1000m);
            entity.ClosingValue.Should().Be(94000m);
            entity.NetValue.Should().Be(94000m);
            entity.IsLocked.Should().Be((byte)1);
            entity.DepreciationPeriod.Should().Be(12);
            entity.DisposalAmount.Should().Be(50000m);
        }

        [Fact]
        public void DepreciationDetails_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DepreciationDetails
            {
                Finyear = null,
                StartDate = null,
                EndDate = null,
                AssetId = null,
                CapitalizationDate = null,
                ExpiryDate = null,
                DepreciationPeriod = null,
                DisposedDate = null,
                DisposalAmount = null
            };

            entity.Finyear.Should().BeNull();
            entity.StartDate.Should().BeNull();
            entity.EndDate.Should().BeNull();
            entity.AssetId.Should().BeNull();
            entity.CapitalizationDate.Should().BeNull();
            entity.ExpiryDate.Should().BeNull();
            entity.DepreciationPeriod.Should().BeNull();
            entity.DisposedDate.Should().BeNull();
            entity.DisposalAmount.Should().BeNull();
        }

        [Fact]
        public void DepreciationDetails_NavigationProperties_ShouldBeAssignable()
        {
            var depType = new MiscMaster();
            var assetMaster = new AssetMasterGenerals();
            var assetGroup = new AssetGroup();
            var depMiscType = new MiscMaster();

            var entity = new DepreciationDetails
            {
                DepType = depType,
                AssetMasterId = assetMaster,
                AssetGroup = assetGroup,
                DepMiscType = depMiscType
            };

            entity.DepType.Should().BeSameAs(depType);
            entity.AssetMasterId.Should().BeSameAs(assetMaster);
            entity.AssetGroup.Should().BeSameAs(assetGroup);
            entity.DepMiscType.Should().BeSameAs(depMiscType);
        }
    }
}
