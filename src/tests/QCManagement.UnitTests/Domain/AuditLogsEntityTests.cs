using MongoDB.Bson;
using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class AuditLogsEntityTests
    {
        [Fact]
        public void AuditLogs_ShouldInheritFromAuditLogBase()
        {
            typeof(AuditLogBase).IsAssignableFrom(typeof(AuditLogs)).Should().BeTrue();
        }

        [Fact]
        public void AuditLogs_Properties_ShouldBeAssignable()
        {
            var entity = new AuditLogs
            {
                Id = ObjectId.GenerateNewId(),
                Action = "Create",
                Details = "Record created",
                Module = "QCManagement"
            };

            entity.Action.Should().Be("Create");
            entity.Details.Should().Be("Record created");
            entity.Module.Should().Be("QCManagement");
            entity.Id.Should().NotBe(ObjectId.Empty);
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
        public void AuditLogs_AuditLogBaseProperties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AuditLogs
            {
                CreatedBy = 1,
                CreatedByName = "admin",
                IPAddress = "127.0.0.1",
                CreatedAt = now
            };

            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("admin");
            entity.IPAddress.Should().Be("127.0.0.1");
            entity.CreatedAt.Should().Be(now);
        }
    }
}
