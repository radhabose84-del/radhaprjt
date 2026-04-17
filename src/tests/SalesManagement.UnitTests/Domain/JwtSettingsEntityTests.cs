using MaintenanceManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class JwtSettingsEntityTests
{
    [Fact]
    public void ShouldNotInheritFromBaseEntity()
    {
        typeof(SalesManagement.Domain.Common.BaseEntity)
            .IsAssignableFrom(typeof(JwtSettings)).Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new JwtSettings
        {
            SecretKey = "my-secret-key-12345",
            Issuer = "https://bsoft.example.com",
            Audience = "bsoft-api",
            ExpiryMinutes = 60,
            EncryptionKey = "my-encryption-key-12345"
        };

        entity.SecretKey.Should().Be("my-secret-key-12345");
        entity.Issuer.Should().Be("https://bsoft.example.com");
        entity.Audience.Should().Be("bsoft-api");
        entity.ExpiryMinutes.Should().Be(60);
        entity.EncryptionKey.Should().Be("my-encryption-key-12345");
    }

    [Fact]
    public void DefaultValues_ShouldBeDefault()
    {
        var entity = new JwtSettings();

        entity.ExpiryMinutes.Should().Be(0);
    }
}
