using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class RoleModuleEntityTests
    {
        [Fact]
        public void RoleModule_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleModule)).Should().BeFalse();
        }

        [Fact]
        public void RoleModule_Properties_ShouldBeAssignable()
        {
            var entity = new RoleModule
            {
                Id = 1,
                RoleId = 10,
                ModuleId = 20
            };

            entity.Id.Should().Be(1);
            entity.RoleId.Should().Be(10);
            entity.ModuleId.Should().Be(20);
        }

        [Fact]
        public void RoleModule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RoleModule
            {
                Role = null,
                Module = null
            };

            entity.Role.Should().BeNull();
            entity.Module.Should().BeNull();
        }

        [Fact]
        public void RoleModule_NavigationProperty_Role_ShouldBeAssignable()
        {
            var role = new UserRole { Id = 3 };
            var entity = new RoleModule { Role = role, RoleId = 3 };

            entity.Role.Should().NotBeNull();
            entity.Role!.Id.Should().Be(3);
        }

        [Fact]
        public void RoleModule_NavigationProperty_Module_ShouldBeAssignable()
        {
            var module = new Modules { Id = 5 };
            var entity = new RoleModule { Module = module, ModuleId = 5 };

            entity.Module.Should().NotBeNull();
            entity.Module!.Id.Should().Be(5);
        }
    }
}
