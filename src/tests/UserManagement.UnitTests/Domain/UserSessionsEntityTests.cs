using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UserSessionsEntityTests
    {
        [Fact]
        public void UserSessions_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserSessions)).Should().BeFalse();
        }

        [Fact]
        public void UserSessions_DefaultIsActive_ShouldBeZero()
        {
            var entity = new UserSessions();
            entity.IsActive.Should().Be(0);
        }

        [Fact]
        public void UserSessions_Properties_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var expires = now.AddHours(1);
            var entity = new UserSessions
            {
                Id = 1,
                UserId = 10,
                JwtId = "jwt-id-123",
                BrowserInfo = "Chrome/120.0",
                CreatedAt = now,
                ExpiresAt = expires,
                IsActive = 1,
                LastActivity = now
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(10);
            entity.JwtId.Should().Be("jwt-id-123");
            entity.BrowserInfo.Should().Be("Chrome/120.0");
            entity.CreatedAt.Should().Be(now);
            entity.ExpiresAt.Should().Be(expires);
            entity.IsActive.Should().Be(1);
            entity.LastActivity.Should().Be(now);
        }

        [Fact]
        public void UserSessions_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserSessions
            {
                JwtId = null,
                BrowserInfo = null
            };

            entity.JwtId.Should().BeNull();
            entity.BrowserInfo.Should().BeNull();
        }
    }
}
