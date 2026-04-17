using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class ExistingVendorDetailsEntityTests
    {
        [Fact]
        public void ExistingVendorDetails_Properties_ShouldBeAssignable()
        {
            var entity = new ExistingVendorDetails
            {
                VendorCode = "V001",
                VendorName = "ABC Supplies",
                VendorEmail = "abc@example.com",
                VendorPhone = "9876543210"
            };

            entity.VendorCode.Should().Be("V001");
            entity.VendorName.Should().Be("ABC Supplies");
            entity.VendorEmail.Should().Be("abc@example.com");
            entity.VendorPhone.Should().Be("9876543210");
        }

        [Fact]
        public void ExistingVendorDetails_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ExistingVendorDetails();

            entity.VendorCode.Should().BeNull();
            entity.VendorName.Should().BeNull();
            entity.VendorEmail.Should().BeNull();
            entity.VendorPhone.Should().BeNull();
        }
    }
}
