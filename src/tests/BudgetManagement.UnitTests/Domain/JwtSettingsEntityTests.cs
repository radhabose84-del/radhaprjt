using BudgetManagement.Domain.Entities;

namespace BudgetManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_ShouldNotInheritFromBaseEntity()
        {
            typeof(BudgetManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(JwtSettings))
                .Should().BeFalse();
        }

        [Fact]
        public void JwtSettings_DefaultStringProperties_ShouldBeNull()
        {
            // default! means null at runtime
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
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var settings = new JwtSettings
            {
                SecretKey = "my-secret-key-256-bit",
                Issuer = "https://bsoft.example.com",
                Audience = "bsoft-api",
                ExpiryMinutes = 60,
                EncryptionKey = "encryption-key-128"
            };

            settings.SecretKey.Should().Be("my-secret-key-256-bit");
            settings.Issuer.Should().Be("https://bsoft.example.com");
            settings.Audience.Should().Be("bsoft-api");
            settings.ExpiryMinutes.Should().Be(60);
            settings.EncryptionKey.Should().Be("encryption-key-128");
        }
    }
}
