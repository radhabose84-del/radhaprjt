using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserUnitEntityTests
    {
        [Fact]
        public void UserUnit_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserUnit)).Should().BeFalse();
        }

        [Fact]
        public void UserUnit_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserUnit();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserUnit_Properties_ShouldBeAssignable()
        {
            var entity = new UserUnit
            {
                Id = 1,
                UserId = 10,
                UnitId = 20,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.UnitId.Should().Be(20);
            entity.IsActive.Should().Be(1);
        }

        [Fact]
        public void UserUnit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserUnit
            {
                User = null,
                Unit = null
            };

            entity.User.Should().BeNull();
            entity.Unit.Should().BeNull();
        }

        [Fact]
        public void UserUnit_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User { UserId = 5 };
            var entity = new UserUnit { User = user, UserId = 5 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(5);
        }

        [Fact]
        public void UserUnit_NavigationProperty_Unit_ShouldBeAssignable()
        {
            var unit = new Unit { Id = 3 };
            var entity = new UserUnit { Unit = unit, UnitId = 3 };

            entity.Unit.Should().NotBeNull();
            entity.Unit!.Id.Should().Be(3);
        }
    }
}
