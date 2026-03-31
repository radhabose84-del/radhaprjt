using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetAmcEntityTests
    {
        [Fact]
        public void AssetAmc_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetAmc();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetAmc_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetAmc();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetAmc_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetAmc)).Should().BeTrue();
        }

        [Fact]
        public void AssetAmc_Properties_ShouldBeAssignable()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var entity = new AssetAmc
            {
                Id = 1,
                AssetId = 5,
                StartDate = today,
                EndDate = today.AddMonths(12),
                Period = 12,
                VendorCode = "VND001",
                VendorName = "Vendor One",
                VendorPhone = "9876543210",
                VendorEmail = "vendor@test.com",
                CoverageType = 1,
                FreeServiceCount = 3,
                RenewalStatus = 1,
                RenewedDate = today
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(5);
            entity.Period.Should().Be(12);
            entity.VendorName.Should().Be("Vendor One");
        }

        [Fact]
        public void AssetAmc_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetAmc
            {
                StartDate = null,
                EndDate = null,
                Period = null,
                VendorCode = null,
                VendorName = null,
                VendorPhone = null,
                VendorEmail = null,
                CoverageType = null,
                FreeServiceCount = null,
                RenewalStatus = null,
                RenewedDate = null
            };

            entity.StartDate.Should().BeNull();
            entity.Period.Should().BeNull();
            entity.VendorCode.Should().BeNull();
        }
    }
}
