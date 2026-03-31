using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetInsuranceEntityTests
    {
        [Fact]
        public void AssetInsurance_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetInsurance();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetInsurance_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetInsurance();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetInsurance_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetInsurance)).Should().BeTrue();
        }

        [Fact]
        public void AssetInsurance_Properties_ShouldBeAssignable()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var entity = new AssetInsurance
            {
                Id = 1,
                AssetId = 10,
                PolicyNo = "POL001",
                StartDate = today,
                Insuranceperiod = 12,
                EndDate = today.AddMonths(12),
                PolicyAmount = 5000m,
                VendorCode = "VND001",
                RenewalStatus = 1,
                RenewedDate = today
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.PolicyNo.Should().Be("POL001");
            entity.Insuranceperiod.Should().Be(12);
            entity.PolicyAmount.Should().Be(5000m);
        }

        [Fact]
        public void AssetInsurance_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetInsurance
            {
                PolicyNo = null,
                PolicyAmount = null,
                VendorCode = null,
                AssetMaster = null
            };

            entity.PolicyNo.Should().BeNull();
            entity.PolicyAmount.Should().BeNull();
            entity.VendorCode.Should().BeNull();
        }
    }
}
