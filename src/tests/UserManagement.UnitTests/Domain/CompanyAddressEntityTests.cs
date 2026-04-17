using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class CompanyAddressEntityTests
    {
        [Fact]
        public void CompanyAddress_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CompanyAddress)).Should().BeFalse();
        }

        [Fact]
        public void CompanyAddress_Properties_ShouldBeAssignable()
        {
            var entity = new CompanyAddress
            {
                Id = 1,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 100",
                PinCode = "600001",
                CountryId = 1,
                StateId = 5,
                CityId = 10,
                Phone = "1234567890",
                AlternatePhone = "0987654321",
                CompanyId = 3
            };

            entity.Id.Should().Be(1);
            entity.AddressLine1.Should().Be("123 Main St");
            entity.AddressLine2.Should().Be("Suite 100");
            entity.PinCode.Should().Be("600001");
            entity.CountryId.Should().Be(1);
            entity.StateId.Should().Be(5);
            entity.CityId.Should().Be(10);
            entity.Phone.Should().Be("1234567890");
            entity.AlternatePhone.Should().Be("0987654321");
            entity.CompanyId.Should().Be(3);
        }

        [Fact]
        public void CompanyAddress_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CompanyAddress
            {
                AddressLine1 = null,
                AddressLine2 = null,
                PinCode = null,
                Phone = null,
                AlternatePhone = null,
                Company = null
            };

            entity.AddressLine1.Should().BeNull();
            entity.AddressLine2.Should().BeNull();
            entity.PinCode.Should().BeNull();
            entity.Phone.Should().BeNull();
            entity.AlternatePhone.Should().BeNull();
            entity.Company.Should().BeNull();
        }

        [Fact]
        public void CompanyAddress_NavigationProperty_Company_ShouldBeAssignable()
        {
            var company = new Company { Id = 5 };
            var entity = new CompanyAddress { Company = company, CompanyId = 5 };

            entity.Company.Should().NotBeNull();
            entity.Company!.Id.Should().Be(5);
        }
    }
}
