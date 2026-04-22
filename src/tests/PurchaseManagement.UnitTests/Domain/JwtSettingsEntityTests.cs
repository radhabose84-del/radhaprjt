using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Domain
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
                SecretKey = "my-secret-key-12345",
                Issuer = "BSOFT.Api",
                Audience = "BSOFT.Client",
                ExpiryMinutes = 60,
                EncryptionKey = "encryption-key-12345"
            };

            entity.SecretKey.Should().Be("my-secret-key-12345");
            entity.Issuer.Should().Be("BSOFT.Api");
            entity.Audience.Should().Be("BSOFT.Client");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("encryption-key-12345");
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
    }
}
