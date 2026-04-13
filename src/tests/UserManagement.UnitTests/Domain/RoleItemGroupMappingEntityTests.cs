using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class RoleItemGroupMappingEntityTests
    {
        [Fact]
        public void RoleItemGroupMapping_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new RoleItemGroupMapping();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void RoleItemGroupMapping_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RoleItemGroupMapping();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RoleItemGroupMapping_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleItemGroupMapping)).Should().BeTrue();
        }

        [Fact]
        public void RoleItemGroupMapping_Properties_ShouldBeAssignable()
        {
            var entity = new RoleItemGroupMapping
            {
                Id = 1,
                RoleId = 5,
                ItemGroupId = 10
            };

            entity.Id.Should().Be(1);
            entity.RoleId.Should().Be(5);
            entity.ItemGroupId.Should().Be(10);
        }

        [Fact]
        public void RoleItemGroupMapping_NavigationProperty_UserRole_ShouldAcceptNull()
        {
            var entity = new RoleItemGroupMapping { UserRole = null };

            entity.UserRole.Should().BeNull();
        }

        [Fact]
        public void RoleItemGroupMapping_NavigationProperty_UserRole_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 5, RoleName = "Admin" };
            var entity = new RoleItemGroupMapping { RoleId = 5, UserRole = role };

            entity.UserRole.Should().NotBeNull();
            entity.UserRole!.RoleName.Should().Be("Admin");
        }
    }
}
