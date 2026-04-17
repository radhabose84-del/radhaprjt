using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.UnitTests.Domain
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
                Issuer = "https://issuer.example.com",
                Audience = "https://audience.example.com",
                ExpiryMinutes = 60,
                EncryptionKey = "enc-key-456"
            };

            entity.SecretKey.Should().Be("my-secret-key-123");
            entity.Issuer.Should().Be("https://issuer.example.com");
            entity.Audience.Should().Be("https://audience.example.com");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("enc-key-456");
        }

        [Fact]
        public void JwtSettings_NullableProperties_ShouldAcceptNull()
        {
            var entity = new JwtSettings
            {
                SecretKey = null!,
                Issuer = null!,
                Audience = null!,
                EncryptionKey = null!
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
