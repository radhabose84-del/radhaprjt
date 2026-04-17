using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class ModulesEntityTests
    {
        [Fact]
        public void Modules_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Modules)).Should().BeFalse();
        }

        [Fact]
        public void Modules_DefaultIsDeleted_ShouldBeFalse()
        {
            var entity = new Modules();
            entity.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Modules_Properties_ShouldBeAssignable()
        {
            var entity = new Modules
            {
                Id = 1,
                ModuleName = "UserManagement",
                IsDeleted = false
            };

            entity.Id.Should().Be(1);
            entity.ModuleName.Should().Be("UserManagement");
            entity.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Modules_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Modules
            {
                ModuleName = null,
                Menus = null,
                RoleModules = null
            };

            entity.ModuleName.Should().BeNull();
            entity.Menus.Should().BeNull();
            entity.RoleModules.Should().BeNull();
        }

        [Fact]
        public void Modules_NavigationProperty_Menus_ShouldBeAssignable()
        {
            var menus = new List<Menu>
            {
                new Menu { Id = 1 },
                new Menu { Id = 2 }
            };

            var entity = new Modules { Menus = menus };

            entity.Menus.Should().NotBeNull();
            entity.Menus.Should().HaveCount(2);
        }

        [Fact]
        public void Modules_NavigationProperty_RoleModules_ShouldBeAssignable()
        {
            var roleModules = new List<RoleModule>
            {
                new RoleModule { Id = 1 },
                new RoleModule { Id = 2 }
            };

            var entity = new Modules { RoleModules = roleModules };

            entity.RoleModules.Should().NotBeNull();
            entity.RoleModules.Should().HaveCount(2);
        }
    }
}
