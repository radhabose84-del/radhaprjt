using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class JwtSettingsEntityTests
    {
        [Fact]
        public void JwtSettings_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(JwtSettings)).Should().BeFalse();
        }

        [Fact]
        public void JwtSettings_Properties_ShouldBeAssignable()
        {
            var entity = new JwtSettings
            {
                SecretKey = "my-secret-key-12345",
                Issuer = "BSOFT",
                Audience = "BSOFT-Client",
                ExpiryMinutes = 60,
                EncryptionKey = "enc-key-12345"
            };
            entity.SecretKey.Should().Be("my-secret-key-12345");
            entity.Issuer.Should().Be("BSOFT");
            entity.Audience.Should().Be("BSOFT-Client");
            entity.ExpiryMinutes.Should().Be(60);
            entity.EncryptionKey.Should().Be("enc-key-12345");
        }

        [Fact]
        public void JwtSettings_DefaultExpiryMinutes_ShouldBeZero()
        {
            var entity = new JwtSettings();
            entity.ExpiryMinutes.Should().Be(0);
        }
    }
}
