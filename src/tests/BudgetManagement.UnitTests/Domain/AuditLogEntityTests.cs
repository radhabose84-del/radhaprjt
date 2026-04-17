using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using MongoDB.Bson;

namespace BudgetManagement.UnitTests.Domain
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
                Details = "Record created",
                Module = "BudgetGroup"
            };

            entity.Id.Should().Be(objectId);
            entity.Action.Should().Be("Create");
            entity.Details.Should().Be("Record created");
            entity.Module.Should().Be("BudgetGroup");
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
        public void AuditLogBase_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AuditLogs
            {
                MachineName = "SERVER01",
                IPAddress = "192.168.1.1",
                OS = "Windows 11",
                Browser = "Chrome",
                CreatedAt = now,
                CreatedBy = 5,
                CreatedByName = "admin"
            };

            entity.MachineName.Should().Be("SERVER01");
            entity.IPAddress.Should().Be("192.168.1.1");
            entity.OS.Should().Be("Windows 11");
            entity.Browser.Should().Be("Chrome");
            entity.CreatedAt.Should().Be(now);
            entity.CreatedBy.Should().Be(5);
            entity.CreatedByName.Should().Be("admin");
        }

        [Fact]
        public void AuditLogBase_NullableProperties_ShouldAcceptNull()
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
