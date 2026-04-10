using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class UserGroupEntityTests
    {
        [Fact]
        public void UserGroup_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserGroup();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UserGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UserGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserGroup)).Should().BeTrue();
        }

        [Fact]
        public void UserGroup_Properties_ShouldBeAssignable()
        {
            var entity = new UserGroup
            {
                Id = 1,
                GroupCode = "GRP01",
                GroupName = "Admin Group"
            };

            entity.Id.Should().Be(1);
            entity.GroupCode.Should().Be("GRP01");
            entity.GroupName.Should().Be("Admin Group");
        }

        [Fact]
        public void UserGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserGroup
            {
                GroupCode = null,
                GroupName = null,
                Users = null
            };

            entity.GroupCode.Should().BeNull();
            entity.GroupName.Should().BeNull();
            entity.Users.Should().BeNull();
        }

        [Fact]
        public void UserGroup_NavigationProperty_Users_ShouldBeAssignable()
        {
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid() },
                new User { Id = Guid.NewGuid() }
            };

            var entity = new UserGroup { Users = users };

            entity.Users.Should().NotBeNull();
            entity.Users.Should().HaveCount(2);
        }
    }
}
