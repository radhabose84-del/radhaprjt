using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
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
            var settings = new JwtSettings
            {
                SecretKey = "my-secret-key-123",
                Issuer = "https://bsoft.com",
                Audience = "https://api.bsoft.com",
                ExpiryMinutes = 60,
                EncryptionKey = "enc-key-456"
            };

            settings.SecretKey.Should().Be("my-secret-key-123");
            settings.Issuer.Should().Be("https://bsoft.com");
            settings.Audience.Should().Be("https://api.bsoft.com");
            settings.ExpiryMinutes.Should().Be(60);
            settings.EncryptionKey.Should().Be("enc-key-456");
        }

        [Fact]
        public void JwtSettings_DefaultStringProperties_ShouldBeNull()
        {
            var settings = new JwtSettings();

            settings.SecretKey.Should().BeNull();
            settings.Issuer.Should().BeNull();
            settings.Audience.Should().BeNull();
            settings.EncryptionKey.Should().BeNull();
        }

        [Fact]
        public void JwtSettings_DefaultExpiryMinutes_ShouldBeZero()
        {
            var settings = new JwtSettings();
            settings.ExpiryMinutes.Should().Be(0);
        }

        [Fact]
        public void JwtSettings_NullableProperties_ShouldAcceptNull()
        {
            var settings = new JwtSettings
            {
                SecretKey = null,
                Issuer = null,
                Audience = null,
                EncryptionKey = null
            };

            settings.SecretKey.Should().BeNull();
            settings.Issuer.Should().BeNull();
            settings.Audience.Should().BeNull();
            settings.EncryptionKey.Should().BeNull();
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_ShouldAcceptNegativeValues()
        {
            var settings = new JwtSettings { ExpiryMinutes = -1 };
            settings.ExpiryMinutes.Should().Be(-1);
        }
    }
}
