using LogisticsManagement.Domain.Common;
using LogisticsManagement.Domain.Entities;

namespace LogisticsManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(JwtSettings)).Should().BeFalse();
        }

        [Fact]
        public void JwtSettings_ShouldNotInheritFromAuditLogBase()
        {
            typeof(AuditLogBase).IsAssignableFrom(typeof(JwtSettings)).Should().BeFalse();
        }

        [Fact]
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var settings = new JwtSettings
            {
                SecretKey = "super-secret-key-12345",
                Issuer = "https://bsoft.example.com",
                Audience = "https://bsoft-api.example.com",
                ExpiryMinutes = 60,
                EncryptionKey = "encryption-key-abcdef"
            };

            settings.SecretKey.Should().Be("super-secret-key-12345");
            settings.Issuer.Should().Be("https://bsoft.example.com");
            settings.Audience.Should().Be("https://bsoft-api.example.com");
            settings.ExpiryMinutes.Should().Be(60);
            settings.EncryptionKey.Should().Be("encryption-key-abcdef");
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_DefaultShouldBeZero()
        {
            var settings = new JwtSettings();
            settings.ExpiryMinutes.Should().Be(0);
        }

        [Fact]
        public void JwtSettings_StringProperties_ShouldBeNullByDefault()
        {
            var settings = new JwtSettings();

            // Properties are nullable strings with no initializer
            settings.SecretKey.Should().BeNull();
            settings.Issuer.Should().BeNull();
            settings.Audience.Should().BeNull();
            settings.EncryptionKey.Should().BeNull();
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_ShouldAcceptLargeValues()
        {
            var settings = new JwtSettings
            {
                ExpiryMinutes = 525600 // one year in minutes
            };

            settings.ExpiryMinutes.Should().Be(525600);
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_ShouldAcceptZero()
        {
            var settings = new JwtSettings
            {
                ExpiryMinutes = 0
            };

            settings.ExpiryMinutes.Should().Be(0);
        }
    }
}
