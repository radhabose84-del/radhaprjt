using MongoDB.Bson;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.UnitTests.Domain
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
                Id = ObjectId.GenerateNewId(),
                Action = "Create",
                Details = "Test details",
                Module = "TestModule"
            };

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
    }
}
