using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
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
                SecretKey = "my-secret-key",
                EncryptionKey = "my-encryption-key",
                Issuer = "https://issuer.example.com",
                Audience = "https://audience.example.com",
                ExpiryMinutes = 60
            };
            entity.SecretKey.Should().Be("my-secret-key");
            entity.EncryptionKey.Should().Be("my-encryption-key");
            entity.Issuer.Should().Be("https://issuer.example.com");
            entity.Audience.Should().Be("https://audience.example.com");
            entity.ExpiryMinutes.Should().Be(60);
        }

        [Fact]
        public void JwtSettings_NullableProperties_ShouldAcceptNull()
        {
            var entity = new JwtSettings
            {
                SecretKey = null,
                EncryptionKey = null,
                Issuer = null,
                Audience = null
            };
            entity.SecretKey.Should().BeNull();
            entity.EncryptionKey.Should().BeNull();
            entity.Issuer.Should().BeNull();
            entity.Audience.Should().BeNull();
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_DefaultShouldBeZero()
        {
            var entity = new JwtSettings();
            entity.ExpiryMinutes.Should().Be(0);
        }
    }
}
