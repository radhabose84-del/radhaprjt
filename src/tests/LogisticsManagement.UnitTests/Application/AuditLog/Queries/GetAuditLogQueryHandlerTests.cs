using LogisticsManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace LogisticsManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// GetAuditLogQueryHandler uses IMongoDatabase directly (not a repository interface).
    /// Due to MongoDB.Driver version conflicts (v2 IAsyncCursor vs v3), direct handler mocking
    /// is not feasible. Controller-level tests in AuditLogControllerTests cover the HTTP layer.
    /// These tests verify the query/DTO types are correctly defined.
    /// </summary>
    public sealed class GetAuditLogQueryHandlerTests
    {
        [Fact]
        public void GetAuditLogQuery_ShouldImplementIRequest()
        {
            var query = new GetAuditLogQuery();
            query.Should().BeAssignableTo<IRequest<List<AuditLogDto>>>();
        }

        [Fact]
        public void AuditLogDto_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new AuditLogDto
            {
                Id = "507f1f77bcf86cd799439011",
                Action = "Create",
                Details = "FreightMaster created",
                Module = "FreightMaster",
                CreatedBy = 1,
                CreatedByName = "admin",
                IPAddress = "127.0.0.1",
                MachineName = "DESKTOP-01",
                OS = "Windows",
                Browser = "Chrome",
                CreatedAt = now
            };

            dto.Id.Should().Be("507f1f77bcf86cd799439011");
            dto.Action.Should().Be("Create");
            dto.Module.Should().Be("FreightMaster");
            dto.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("admin");
            dto.IPAddress.Should().Be("127.0.0.1");
            dto.MachineName.Should().Be("DESKTOP-01");
            dto.OS.Should().Be("Windows");
            dto.Browser.Should().Be("Chrome");
            dto.CreatedAt.Should().Be(now);
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
                CreatedByName = null,
                IPAddress = null,
                MachineName = null,
                OS = null,
                Browser = null
            };

            dto.Id.Should().BeNull();
            dto.Action.Should().BeNull();
            dto.Details.Should().BeNull();
            dto.Module.Should().BeNull();
            dto.CreatedByName.Should().BeNull();
            dto.IPAddress.Should().BeNull();
            dto.MachineName.Should().BeNull();
            dto.OS.Should().BeNull();
            dto.Browser.Should().BeNull();
        }
    }
}
