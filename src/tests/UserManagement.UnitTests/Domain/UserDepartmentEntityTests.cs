using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserDepartmentEntityTests
    {
        [Fact]
        public void UserDepartment_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserDepartment)).Should().BeFalse();
        }

        [Fact]
        public void UserDepartment_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserDepartment();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserDepartment_Properties_ShouldBeAssignable()
        {
            var entity = new UserDepartment
            {
                Id = 1,
                UserId = 10,
                DepartmentId = 20,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.DepartmentId.Should().Be(20);
            entity.IsActive.Should().Be(1);
        }

        [Fact]
        public void UserDepartment_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserDepartment
            {
                User = null,
                Department = null
            };

            entity.User.Should().BeNull();
            entity.Department.Should().BeNull();
        }

        [Fact]
        public void UserDepartment_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User { UserId = 5 };
            var entity = new UserDepartment { User = user, UserId = 5 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(5);
        }

        [Fact]
        public void UserDepartment_NavigationProperty_Department_ShouldBeAssignable()
        {
            var dept = new UserManagement.Domain.Entities.Department { Id = 3 };
            var entity = new UserDepartment { Department = dept, DepartmentId = 3 };

            entity.Department.Should().NotBeNull();
            entity.Department!.Id.Should().Be(3);
        }
    }
}
