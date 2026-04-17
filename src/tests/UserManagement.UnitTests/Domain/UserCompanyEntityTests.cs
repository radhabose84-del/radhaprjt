using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserCompanyEntityTests
    {
        [Fact]
        public void UserCompany_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserCompany)).Should().BeFalse();
        }

        [Fact]
        public void UserCompany_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserCompany();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserCompany_Properties_ShouldBeAssignable()
        {
            var entity = new UserCompany
            {
                Id = 1,
                UserId = 10,
                CompanyId = 20,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.CompanyId.Should().Be(20);
            entity.IsActive.Should().Be(1);
        }

        [Fact]
        public void UserCompany_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserCompany
            {
                User = null,
                Company = null
            };

            entity.User.Should().BeNull();
            entity.Company.Should().BeNull();
        }

        [Fact]
        public void UserCompany_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User { UserId = 5 };
            var entity = new UserCompany { User = user, UserId = 5 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(5);
        }

        [Fact]
        public void UserCompany_NavigationProperty_Company_ShouldBeAssignable()
        {
            var company = new Company { Id = 3 };
            var entity = new UserCompany { Company = company, CompanyId = 3 };

            entity.Company.Should().NotBeNull();
            entity.Company!.Id.Should().Be(3);
        }
    }
}
