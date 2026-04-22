using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.UnitTests.Domain
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
                SecretKey = "SuperSecretKey12345",
                Issuer = "https://bsoft.example.com",
                Audience = "https://api.bsoft.example.com",
                ExpiryMinutes = 60,
                EncryptionKey = "AES256EncryptionKey"
            };

            entity.SecretKey.Should().Be("SuperSecretKey12345");
            entity.Issuer.Should().Be("https://bsoft.example.com");
            entity.Audience.Should().Be("https://api.bsoft.example.com");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("AES256EncryptionKey");
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
        public void JwtSettings_DefaultValues_ShouldBeZeroOrNull()
        {
            var entity = new JwtSettings();

            entity.ExpiryMinutes.Should().Be(0);
            entity.SecretKey.Should().BeNull();
            entity.Issuer.Should().BeNull();
            entity.Audience.Should().BeNull();
            entity.EncryptionKey.Should().BeNull();
        }
    }
}
