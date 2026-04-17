using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class RoleChildEntityTests
    {
        [Fact]
        public void RoleChild_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleChild)).Should().BeFalse();
        }

        [Fact]
        public void RoleChild_Properties_ShouldBeAssignable()
        {
            var entity = new RoleChild
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
        public void RoleChild_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RoleChild
            {
                Role = null,
                Menu = null
            };

            entity.Role.Should().BeNull();
            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void RoleChild_NavigationProperty_Role_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 3 };
            var entity = new RoleChild { Role = role, RoleId = 3 };

            entity.Role.Should().NotBeNull();
            entity.Role!.Id.Should().Be(3);
        }

        [Fact]
        public void RoleChild_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 7 };
            var entity = new RoleChild { Menu = menu, MenuId = 7 };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.Id.Should().Be(7);
        }
    }
}
