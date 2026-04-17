using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class PasswordLogEntityTests
    {
        [Fact]
        public void PasswordLog_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PasswordLog)).Should().BeFalse();
        }

        [Fact]
        public void PasswordLog_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new PasswordLog
            {
                Id = 1,
                UserId = 10,
                UserName = "admin",
                PasswordHash = "hashed-password-123",
                CreatedAt = now,
                CreatedIP = "192.168.1.1"
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.UserName.Should().Be("admin");
            entity.PasswordHash.Should().Be("hashed-password-123");
            entity.CreatedAt.Should().Be(now);
            entity.CreatedIP.Should().Be("192.168.1.1");
        }

        [Fact]
        public void PasswordLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PasswordLog
            {
                UserName = null,
                PasswordHash = null,
                CreatedIP = null,
                User = null
            };

            entity.UserName.Should().BeNull();
            entity.PasswordHash.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
            entity.User.Should().BeNull();
        }

        [Fact]
        public void PasswordLog_NavigationProperty_User_ShouldBeAssignable()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, UserId = 5 };
            var entity = new PasswordLog { User = user, UserId = 5 };

            entity.User.Should().NotBeNull();
            entity.User!.UserId.Should().Be(5);
        }
    }
}
