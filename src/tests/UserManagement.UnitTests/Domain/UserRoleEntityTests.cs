using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class UserRoleEntityTests
    {
        [Fact]
        public void UserRole_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserRole();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UserRole_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserRole();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UserRole_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserRole)).Should().BeTrue();
        }

        [Fact]
        public void UserRole_Properties_ShouldBeAssignable()
        {
            var entity = new UserRole
            {
                Id = 1,
                RoleName = "Administrator",
                Description = "Full access role",
                CompanyId = 5,
                BypassDataAccess = true
            };

            entity.Id.Should().Be(1);
            entity.RoleName.Should().Be("Administrator");
            entity.Description.Should().Be("Full access role");
            entity.CompanyId.Should().Be(5);
            entity.BypassDataAccess.Should().BeTrue();
        }

        [Fact]
        public void UserRole_DefaultBypassDataAccess_ShouldBeFalse()
        {
            var entity = new UserRole();
            entity.BypassDataAccess.Should().BeFalse();
        }

        [Fact]
        public void UserRole_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserRole
            {
                RoleName = null,
                Description = null,
                UserRoleAllocations = null,
                RoleModules = null,
                RoleParents = null,
                RoleChildren = null,
                RoleMenuPrivileges = null,
                RoleItemGroupMappings = null
            };

            entity.RoleName.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.UserRoleAllocations.Should().BeNull();
            entity.RoleModules.Should().BeNull();
            entity.RoleMenuPrivileges.Should().BeNull();
            entity.RoleItemGroupMappings.Should().BeNull();
        }

        [Fact]
        public void UserRole_NavigationProperty_RoleItemGroupMappings_ShouldBeAssignable()
        {
            var mappings = new List<RoleItemGroupMapping>
            {
                new RoleItemGroupMapping { Id = 1, RoleId = 5, ItemGroupId = 10 },
                new RoleItemGroupMapping { Id = 2, RoleId = 5, ItemGroupId = 20 }
            };

            var entity = new UserRole { Id = 5, RoleItemGroupMappings = mappings };

            entity.RoleItemGroupMappings.Should().HaveCount(2);
        }
    }
}
