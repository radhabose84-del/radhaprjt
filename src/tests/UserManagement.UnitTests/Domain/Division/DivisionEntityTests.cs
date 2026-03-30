using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain.Division
{
    public class DivisionEntityTests
    {
        [Fact]
        public void Division_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserManagement.Domain.Entities.Division();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Division_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserManagement.Domain.Entities.Division();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Division_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserManagement.Domain.Entities.Division)).Should().BeTrue();
        }

        [Fact]
        public void Division_Properties_ShouldBeAssignable()
        {
            var entity = new UserManagement.Domain.Entities.Division
            {
                Id = 1,
                ShortName = "DIV01",
                Name = "Test Division",
                CompanyId = 5
            };

            entity.Id.Should().Be(1);
            entity.ShortName.Should().Be("DIV01");
            entity.Name.Should().Be("Test Division");
            entity.CompanyId.Should().Be(5);
        }

        [Fact]
        public void Division_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserManagement.Domain.Entities.Division
            {
                ShortName = null,
                Name = null,
                Company = null,
                UserDivisions = null,
                Units = null
            };

            entity.ShortName.Should().BeNull();
            entity.Name.Should().BeNull();
            entity.Company.Should().BeNull();
            entity.UserDivisions.Should().BeNull();
            entity.Units.Should().BeNull();
        }

        [Fact]
        public void Division_NavigationProperty_Company_ShouldBeAssignable()
        {
            var company = new Company
            {
                Id = 1,
                CompanyName = "Test Company"
            };

            var entity = new UserManagement.Domain.Entities.Division
            {
                Id = 1,
                CompanyId = 1,
                Company = company
            };

            entity.Company.Should().NotBeNull();
            entity.Company!.CompanyName.Should().Be("Test Company");
        }

        [Fact]
        public void Division_NavigationProperty_Units_ShouldBeAssignable()
        {
            var units = new List<Unit>
            {
                new Unit { Id = 1 },
                new Unit { Id = 2 }
            };

            var entity = new UserManagement.Domain.Entities.Division
            {
                Id = 1,
                Units = units
            };

            entity.Units.Should().NotBeNull();
            entity.Units.Should().HaveCount(2);
        }
    }
}
