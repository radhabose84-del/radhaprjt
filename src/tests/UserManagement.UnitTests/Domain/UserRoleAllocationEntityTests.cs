using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserRoleAllocationEntityTests
    {
        [Fact]
        public void UserRoleAllocation_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserRoleAllocation)).Should().BeFalse();
        }

        [Fact]
        public void UserRoleAllocation_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserRoleAllocation();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserRoleAllocation_Properties_ShouldBeAssignable()
        {
            var entity = new UserRoleAllocation
            {
                Id = 1,
                UserRoleId = 10,
                UserId = 20,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.UserRoleId.Should().Be(10);
            entity.UserId.Should().Be(20);
            entity.IsActive.Should().Be(1);
        }

        [Fact]
        public void UserRoleAllocation_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserRoleAllocation
            {
                UserRole = null,
                User = null
            };

            entity.UserRole.Should().BeNull();
            entity.User.Should().BeNull();
        }

        [Fact]
        public void UserRoleAllocation_NavigationProperty_UserRole_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 5 };
            var entity = new UserRoleAllocation { UserRole = role, UserRoleId = 5 };

            entity.UserRole.Should().NotBeNull();
            entity.UserRole!.Id.Should().Be(5);
        }

        [Fact]
        public void UserRoleAllocation_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User { UserId = 3 };
            var entity = new UserRoleAllocation { User = user, UserId = 3 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(3);
        }
    }
}
