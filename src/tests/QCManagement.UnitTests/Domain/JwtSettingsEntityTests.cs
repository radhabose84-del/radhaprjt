using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var entity = new JwtSettings
            {
                SecretKey = "super-secret-key",
                Issuer = "bsoft-issuer",
                Audience = "bsoft-audience",
                ExpiryMinutes = 60,
                EncryptionKey = "enc-key-123"
            };

            entity.SecretKey.Should().Be("super-secret-key");
            entity.Issuer.Should().Be("bsoft-issuer");
            entity.Audience.Should().Be("bsoft-audience");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("enc-key-123");
        }

        [Fact]
        public void JwtSettings_ExpiryMinutes_DefaultIsZero()
        {
            var entity = new JwtSettings();
            entity.ExpiryMinutes.Should().Be(0);
        }

        [Fact]
        public void JwtSettings_NullableStringProperties_ShouldAcceptNull()
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
    }
}
