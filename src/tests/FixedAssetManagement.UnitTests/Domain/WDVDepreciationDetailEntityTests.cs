using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class WDVDepreciationDetailEntityTests
    {
        [Fact]
        public void WDVDepreciationDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new WDVDepreciationDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WDVDepreciationDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new WDVDepreciationDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WDVDepreciationDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(WDVDepreciationDetail)).Should().BeTrue();
        }

        [Fact]
        public void WDVDepreciationDetail_Properties_ShouldBeAssignable()
        {
            var entity = new WDVDepreciationDetail
            {
                Id = 1,
                CompanyId = 10,
                FinYear = 2025,
                AssetGroupId = 5,
                AssetSubGroupId = 3,
                DepreciationPercentage = 15.5m,
                OpeningValue = 100000m,
                LastYearAdditionalDepreciation = 5000m,
                LessThan180DaysValue = 20000m,
                MoreThan180DaysValue = 50000m,
                DeletionValue = 1000m,
                ClosingValue = 80000m,
                DepreciationValue = 12000m,
                AdditionalDepreciationValue = 3000m,
                WDVDepreciationValue = 15000m,
                AdditionalCarryForward = 2000m,
                CapitalGainLossValue = 500m,
                IsLocked = 0,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddYears(1)
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(10);
            entity.FinYear.Should().Be(2025);
            entity.AssetGroupId.Should().Be(5);
            entity.DepreciationPercentage.Should().Be(15.5m);
            entity.OpeningValue.Should().Be(100000m);
            entity.WDVDepreciationValue.Should().Be(15000m);
            entity.IsLocked.Should().Be(0);
        }

        [Fact]
        public void WDVDepreciationDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WDVDepreciationDetail
            {
                FinYear = null,
                AssetSubGroupId = null,
                StartDate = null,
                EndDate = null
            };

            entity.FinYear.Should().BeNull();
            entity.AssetSubGroupId.Should().BeNull();
            entity.StartDate.Should().BeNull();
            entity.EndDate.Should().BeNull();
        }
    }
}
