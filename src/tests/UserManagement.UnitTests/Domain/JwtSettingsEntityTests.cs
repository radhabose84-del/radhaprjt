using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(JwtSettings)).Should().BeFalse();
        }

        [Fact]
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var entity = new JwtSettings
            {
                SecretKey = "my-secret-key-123",
                Issuer = "https://bsoft.com",
                Audience = "https://bsoft.com/api",
                ExpiryMinutes = 60,
                EncryptionKey = "encryption-key-456"
            };

            entity.SecretKey.Should().Be("my-secret-key-123");
            entity.Issuer.Should().Be("https://bsoft.com");
            entity.Audience.Should().Be("https://bsoft.com/api");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("encryption-key-456");
        }

        [Fact]
        public void JwtSettings_NullableProperties_ShouldAcceptNull()
        {
            var entity = new JwtSettings
            {
                SecretKey = null,
                Issuer = null,
                Audience = null,
                EncryptionKey = null
            };

            entity.SecretKey.Should().BeNull();
            entity.Issuer.Should().BeNull();
            entity.Audience.Should().BeNull();
            entity.EncryptionKey.Should().BeNull();
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_DefaultShouldBeZero()
        {
            var entity = new JwtSettings();
            entity.ExpiryMinutes.Should().Be(0);
        }
    }
}
