using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using MongoDB.Bson;

namespace ProjectManagement.UnitTests.Domain
{
    public class AuditLogEntityTests
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
            var objectId = ObjectId.GenerateNewId();
            var entity = new AuditLogs
            {
                Id = objectId,
                Action = "Create",
                Details = "Test details",
                Module = "TestModule"
            };

            entity.Id.Should().Be(objectId);
            entity.Action.Should().Be("Create");
            entity.Details.Should().Be("Test details");
            entity.Module.Should().Be("TestModule");
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
        public void AuditLogs_InheritedAuditLogBaseProperties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AuditLogs
            {
                MachineName = "SERVER01",
                IPAddress = "192.168.1.1",
                OS = "Windows",
                Browser = "Chrome",
                CreatedAt = now,
                CreatedBy = 42,
                CreatedByName = "admin"
            };

            entity.MachineName.Should().Be("SERVER01");
            entity.IPAddress.Should().Be("192.168.1.1");
            entity.OS.Should().Be("Windows");
            entity.Browser.Should().Be("Chrome");
            entity.CreatedAt.Should().Be(now);
            entity.CreatedBy.Should().Be(42);
            entity.CreatedByName.Should().Be("admin");
        }

        [Fact]
        public void AuditLogs_InheritedNullableProperties_ShouldAcceptNull()
        {
            var entity = new AuditLogs
            {
                MachineName = null,
                IPAddress = null,
                OS = null,
                Browser = null,
                CreatedByName = null
            };

            entity.MachineName.Should().BeNull();
            entity.IPAddress.Should().BeNull();
            entity.OS.Should().BeNull();
            entity.Browser.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
        }
    }
}
