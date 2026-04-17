using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class CompanyContactEntityTests
    {
        [Fact]
        public void CompanyContact_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CompanyContact)).Should().BeFalse();
        }

        [Fact]
        public void CompanyContact_Properties_ShouldBeAssignable()
        {
            var entity = new CompanyContact
            {
                Id = 1,
                Name = "John Doe",
                Designation = "Manager",
                Email = "john@test.com",
                Phone = "1234567890",
                Remarks = "Primary contact",
                CompanyId = 3
            };

            entity.Id.Should().Be(1);
            entity.Name.Should().Be("John Doe");
            entity.Designation.Should().Be("Manager");
            entity.Email.Should().Be("john@test.com");
            entity.Phone.Should().Be("1234567890");
            entity.Remarks.Should().Be("Primary contact");
            entity.CompanyId.Should().Be(3);
        }

        [Fact]
        public void CompanyContact_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CompanyContact
            {
                Name = null,
                Designation = null,
                Email = null,
                Phone = null,
                Remarks = null,
                Company = null
            };

            entity.Name.Should().BeNull();
            entity.Designation.Should().BeNull();
            entity.Email.Should().BeNull();
            entity.Phone.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.Company.Should().BeNull();
        }

        [Fact]
        public void CompanyContact_NavigationProperty_Company_ShouldBeAssignable()
        {
            var company = new Company { Id = 2 };
            var entity = new CompanyContact { Company = company, CompanyId = 2 };

            entity.Company.Should().NotBeNull();
            entity.Company!.Id.Should().Be(2);
        }
    }
}
