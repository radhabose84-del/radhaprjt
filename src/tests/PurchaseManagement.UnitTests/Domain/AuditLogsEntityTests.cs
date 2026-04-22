using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Domain
{
    public class AuditLogsEntityTests
    {
        [Fact]
        public void AuditLogs_ShouldInheritFromAuditLogBase()
        {
            typeof(AuditLogBase).IsAssignableFrom(typeof(AuditLogs)).Should().BeTrue();
        }

        [Fact]
        public void AuditLogs_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AuditLogs)).Should().BeFalse();
        }

        [Fact]
        public void AuditLogs_Properties_ShouldBeAssignable()
        {
            var entity = new AuditLogs
            {
                Action = "Create",
                Details = "Record created",
                Module = "PurchaseManagement"
            };

            entity.Action.Should().Be("Create");
            entity.Details.Should().Be("Record created");
            entity.Module.Should().Be("PurchaseManagement");
        }

        [Fact]
        public void AuditLogs_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AuditLogs
            {
                Action = null,
                Details = null,
                Module = null
            };

            entity.Action.Should().BeNull();
            entity.Details.Should().BeNull();
            entity.Module.Should().BeNull();
        }

        [Fact]
        public void AuditLogs_InheritedProperties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AuditLogs
            {
                MachineName = "SERVER01",
                IPAddress = "192.168.1.1",
                OS = "Windows",
                Browser = "Chrome",
                CreatedAt = now,
                CreatedBy = 1,
                CreatedByName = "admin"
            };

            entity.MachineName.Should().Be("SERVER01");
            entity.IPAddress.Should().Be("192.168.1.1");
            entity.OS.Should().Be("Windows");
            entity.Browser.Should().Be("Chrome");
            entity.CreatedAt.Should().Be(now);
            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("admin");
        }
    }
}
