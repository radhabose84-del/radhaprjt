using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.UnitTests.Domain
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
                SecretKey = "my-secret",
                Issuer = "BSOFT",
                Audience = "BSOFT-Client",
                ExpiryMinutes = 60,
                EncryptionKey = "enc-key"
            };

            entity.SecretKey.Should().Be("my-secret");
            entity.Issuer.Should().Be("BSOFT");
            entity.ExpiryMinutes.Should().Be(60);
        }

        [Fact]
        public void JwtSettings_DefaultExpiryMinutes_ShouldBeZero()
        {
            var entity = new JwtSettings();
            entity.ExpiryMinutes.Should().Be(0);
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
            entity.EncryptionKey.Should().BeNull();
        }
    }
}
