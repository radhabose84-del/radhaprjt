using FAM.Domain.Entities;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var entity = new JwtSettings
            {
                SecretKey = "my-secret-key-12345",
                Issuer = "https://bsoft.com",
                Audience = "https://bsoft.com/api",
                ExpiryMinutes = 60,
                EncryptionKey = "encryption-key-12345"
            };

            entity.SecretKey.Should().Be("my-secret-key-12345");
            entity.Issuer.Should().Be("https://bsoft.com");
            entity.Audience.Should().Be("https://bsoft.com/api");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("encryption-key-12345");
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_DefaultShouldBeZero()
        {
            var entity = new JwtSettings();

            entity.ExpiryMinutes.Should().Be(0);
        }
    }
}
