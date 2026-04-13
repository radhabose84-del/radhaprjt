using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class EntityEntityTests
    {
        [Fact]
        public void Entity_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserManagement.Domain.Entities.Entity();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserManagement.Domain.Entities.Entity();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Entity_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserManagement.Domain.Entities.Entity)).Should().BeTrue();
        }

        [Fact]
        public void Entity_Properties_ShouldBeAssignable()
        {
            var entity = new UserManagement.Domain.Entities.Entity
            {
                Id = 1,
                EntityCode = "ENT01",
                EntityName = "Test Entity",
                EntityDescription = "Test Description",
                Address = "Test Address",
                Phone = "1234567890",
                Email = "test@test.com"
            };

            entity.Id.Should().Be(1);
            entity.EntityCode.Should().Be("ENT01");
            entity.EntityName.Should().Be("Test Entity");
            entity.EntityDescription.Should().Be("Test Description");
            entity.Address.Should().Be("Test Address");
            entity.Phone.Should().Be("1234567890");
            entity.Email.Should().Be("test@test.com");
        }

        [Fact]
        public void Entity_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserManagement.Domain.Entities.Entity
            {
                EntityCode = null,
                EntityName = null,
                EntityDescription = null,
                Address = null,
                Phone = null,
                Email = null,
                AdminSecuritySettings = null
            };

            entity.EntityCode.Should().BeNull();
            entity.EntityName.Should().BeNull();
            entity.AdminSecuritySettings.Should().BeNull();
        }

        [Fact]
        public void Entity_NavigationProperty_AdminSecuritySettings_ShouldBeAssignable()
        {
            var settings = new AdminSecuritySettings { Id = 5, PasswordExpiryDays = 90 };
            var entity = new UserManagement.Domain.Entities.Entity
            {
                Id = 1,
                AdminSecuritySettings = settings
            };

            entity.AdminSecuritySettings.Should().NotBeNull();
            entity.AdminSecuritySettings!.PasswordExpiryDays.Should().Be(90);
        }
    }
}
