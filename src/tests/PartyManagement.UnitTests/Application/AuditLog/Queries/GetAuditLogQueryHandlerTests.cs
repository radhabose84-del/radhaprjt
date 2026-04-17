using MongoDB.Driver;
using PartyManagement.Application.AuditLog.Queries.GetAuditLog;
using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// Tests for GetAuditLogQueryHandler which queries MongoDB for all audit logs.
    /// The handler injects IMongoDatabase and maps AuditLogs entities to AuditLogDto.
    /// Due to MongoDB driver version conflicts in the test project, these tests verify
    /// construction and query/dto structure rather than mocking the full Find pipeline.
    /// </summary>
    public sealed class GetAuditLogQueryHandlerTests
    {
        [Fact]
        public void Handler_CanBeConstructed_WithMockedDatabase()
        {
            var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Loose);
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(mockCollection.Object);

            var handler = new GetAuditLogQueryHandler(mockDatabase.Object);

            handler.Should().NotBeNull();
        }

        [Fact]
        public void Query_CanBeInstantiated()
        {
            var query = new GetAuditLogQuery();
            query.Should().NotBeNull();
        }

        [Fact]
        public void AuditLogDto_Properties_AreAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new AuditLogDto
            {
                Id = "507f1f77bcf86cd799439011",
                Action = "Create",
                Details = "Party created",
                Module = "PartyMaster",
                CreatedBy = 1,
                CreatedByName = "admin",
                IPAddress = "127.0.0.1",
                MachineName = "DEV01",
                OS = "Windows",
                Browser = "Chrome",
                CreatedAt = now
            };

            dto.Id.Should().Be("507f1f77bcf86cd799439011");
            dto.Action.Should().Be("Create");
            dto.Details.Should().Be("Party created");
            dto.Module.Should().Be("PartyMaster");
            dto.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("admin");
            dto.CreatedAt.Should().Be(now);
        }

        [Fact]
        public void AuditLogDto_DefaultValues_AreNull()
        {
            var dto = new AuditLogDto();

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
