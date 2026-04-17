using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ExistingVendorDetailsEntityTests
    {
        [Fact]
        public void ExistingVendorDetails_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ExistingVendorDetails)).Should().BeFalse();
        }

        [Fact]
        public void ExistingVendorDetails_Properties_ShouldBeAssignable()
        {
            var entity = new ExistingVendorDetails
            {
                VendorCode = "V001",
                VendorName = "Vendor A",
                VendorEmail = "vendor@test.com",
                VendorPhone = "9876543210"
            };
            entity.VendorCode.Should().Be("V001");
            entity.VendorName.Should().Be("Vendor A");
            entity.VendorEmail.Should().Be("vendor@test.com");
            entity.VendorPhone.Should().Be("9876543210");
        }

        [Fact]
        public void ExistingVendorDetails_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ExistingVendorDetails
            {
                VendorCode = null,
                VendorName = null,
                VendorEmail = null,
                VendorPhone = null
            };
            entity.VendorCode.Should().BeNull();
            entity.VendorName.Should().BeNull();
            entity.VendorEmail.Should().BeNull();
            entity.VendorPhone.Should().BeNull();
        }
    }
}
