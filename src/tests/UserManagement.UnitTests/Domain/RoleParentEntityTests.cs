using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class RoleParentEntityTests
    {
        [Fact]
        public void RoleParent_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleParent)).Should().BeFalse();
        }

        [Fact]
        public void RoleParent_Properties_ShouldBeAssignable()
        {
            var entity = new RoleParent
            {
                Id = 1,
                RoleId = 10,
                MenuId = 20
            };

            entity.Id.Should().Be(1);
            entity.RoleId.Should().Be(10);
            entity.MenuId.Should().Be(20);
        }

        [Fact]
        public void RoleParent_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RoleParent
            {
                Role = null,
                Menu = null
            };

            entity.Role.Should().BeNull();
            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void RoleParent_NavigationProperty_Role_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 4 };
            var entity = new RoleParent { Role = role, RoleId = 4 };

            entity.Role.Should().NotBeNull();
            entity.Role!.Id.Should().Be(4);
        }

        [Fact]
        public void RoleParent_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 9 };
            var entity = new RoleParent { Menu = menu, MenuId = 9 };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.Id.Should().Be(9);
        }
    }
}
