using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class RoleMenuPrivilegesEntityTests
    {
        [Fact]
        public void RoleMenuPrivileges_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleMenuPrivileges)).Should().BeFalse();
        }

        [Fact]
        public void RoleMenuPrivileges_Properties_ShouldBeAssignable()
        {
            var entity = new RoleMenuPrivileges
            {
                Id = 1,
                RoleId = 10,
                MenuId = 20,
                CanView = true,
                CanAdd = true,
                CanUpdate = false,
                CanDelete = false,
                CanExport = true,
                CanApprove = false
            };

            entity.Id.Should().Be(1);
            entity.RoleId.Should().Be(10);
            entity.MenuId.Should().Be(20);
            entity.CanView.Should().BeTrue();
            entity.CanAdd.Should().BeTrue();
            entity.CanUpdate.Should().BeFalse();
            entity.CanDelete.Should().BeFalse();
            entity.CanExport.Should().BeTrue();
            entity.CanApprove.Should().BeFalse();
        }

        [Fact]
        public void RoleMenuPrivileges_DefaultBooleans_ShouldBeFalse()
        {
            var entity = new RoleMenuPrivileges();

            entity.CanView.Should().BeFalse();
            entity.CanAdd.Should().BeFalse();
            entity.CanUpdate.Should().BeFalse();
            entity.CanDelete.Should().BeFalse();
            entity.CanExport.Should().BeFalse();
            entity.CanApprove.Should().BeFalse();
        }

        [Fact]
        public void RoleMenuPrivileges_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RoleMenuPrivileges
            {
                UserRole = null,
                Menu = null
            };

            entity.UserRole.Should().BeNull();
            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void RoleMenuPrivileges_NavigationProperty_UserRole_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 5 };
            var entity = new RoleMenuPrivileges { UserRole = role, RoleId = 5 };

            entity.UserRole.Should().NotBeNull();
            entity.UserRole!.Id.Should().Be(5);
        }

        [Fact]
        public void RoleMenuPrivileges_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 8 };
            var entity = new RoleMenuPrivileges { Menu = menu, MenuId = 8 };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.Id.Should().Be(8);
        }
    }
}
