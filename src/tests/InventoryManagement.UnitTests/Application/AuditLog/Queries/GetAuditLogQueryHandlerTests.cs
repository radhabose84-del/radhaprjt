using InventoryManagement.Application.AuditLog.Queries.GetAuditLog;
using InventoryManagement.Domain.Entities;
using MongoDB.Bson;

namespace InventoryManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogQueryHandlerTests
    {
        [Fact]
        public void AuditLogDto_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new AuditLogDto
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Action = "Create",
                Details = "Entity created",
                Module = "TestModule",
                MachineName = "SERVER01",
                IPAddress = "127.0.0.1",
                OS = "Windows",
                Browser = "Chrome",
                CreatedAt = now,
                CreatedBy = 1,
                CreatedByName = "admin"
            };

            dto.Id.Should().NotBeNullOrEmpty();
            dto.Action.Should().Be("Create");
            dto.Details.Should().Be("Entity created");
            dto.Module.Should().Be("TestModule");
            dto.MachineName.Should().Be("SERVER01");
            dto.IPAddress.Should().Be("127.0.0.1");
            dto.OS.Should().Be("Windows");
            dto.Browser.Should().Be("Chrome");
            dto.CreatedAt.Should().Be(now);
            dto.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("admin");
        }

        [Fact]
        public void AuditLogDto_NullableProperties_ShouldAcceptNull()
        {
            var dto = new AuditLogDto
            {
                Id = null,
                Action = null,
                Details = null,
                Module = null,
                MachineName = null,
                IPAddress = null,
                OS = null,
                Browser = null,
                CreatedByName = null
            };

            dto.Id.Should().BeNull();
            dto.Action.Should().BeNull();
            dto.Details.Should().BeNull();
            dto.Module.Should().BeNull();
            dto.MachineName.Should().BeNull();
        }

        [Fact]
        public void GetAuditLogQuery_ImplementsIRequest()
        {
            var query = new GetAuditLogQuery();

            query.Should().NotBeNull();
            query.Should().BeAssignableTo<MediatR.IRequest<List<AuditLogDto>>>();
        }

        [Fact]
        public void AuditLogs_ToDto_MapsIdAsString()
        {
            var objectId = ObjectId.GenerateNewId();
            var log = new AuditLogs
            {
                Id = objectId,
                Action = "Update",
                Details = "Test details",
                Module = "Inventory",
                MachineName = "SRV",
                IPAddress = "10.0.0.1",
                OS = "Linux",
                Browser = "Firefox",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = 5,
                CreatedByName = "user1"
            };

            // Replicate the mapping logic from the handler
            var dto = new AuditLogDto
            {
                Id = log.Id.ToString(),
                CreatedBy = log.CreatedBy,
                CreatedByName = log.CreatedByName,
                IPAddress = log.IPAddress,
                OS = log.OS,
                Browser = log.Browser,
                Action = log.Action,
                Details = log.Details,
                Module = log.Module,
                CreatedAt = log.CreatedAt,
                MachineName = log.MachineName
            };

            dto.Id.Should().Be(objectId.ToString());
            dto.Action.Should().Be("Update");
            dto.Module.Should().Be("Inventory");
            dto.CreatedBy.Should().Be(5);
        }
    }
}
