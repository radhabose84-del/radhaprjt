using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserDivisionEntityTests
    {
        [Fact]
        public void UserDivision_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserDivision)).Should().BeFalse();
        }

        [Fact]
        public void UserDivision_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserDivision();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserDivision_Properties_ShouldBeAssignable()
        {
            var entity = new UserDivision
            {
                Id = 1,
                UserId = 10,
                DivisionId = 20,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.DivisionId.Should().Be(20);
            entity.IsActive.Should().Be(1);
        }

        [Fact]
        public void UserDivision_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserDivision
            {
                User = null,
                Division = null
            };

            entity.User.Should().BeNull();
            entity.Division.Should().BeNull();
        }

        [Fact]
        public void UserDivision_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User { UserId = 5 };
            var entity = new UserDivision { User = user, UserId = 5 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(5);
        }

        [Fact]
        public void UserDivision_NavigationProperty_Division_ShouldBeAssignable()
        {
            var division = new UserManagement.Domain.Entities.Division { Id = 3 };
            var entity = new UserDivision { Division = division, DivisionId = 3 };

            entity.Division.Should().NotBeNull();
            entity.Division!.Id.Should().Be(3);
        }
    }
}
