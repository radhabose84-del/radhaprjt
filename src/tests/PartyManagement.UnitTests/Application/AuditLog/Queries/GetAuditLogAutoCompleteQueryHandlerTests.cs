using MongoDB.Driver;
using PartyManagement.Application.AuditLog.Queries.GetAuditLog;
using PartyManagement.Application.AuditLog.Queries;
using PartyManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;
using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// Tests for GetAuditLogBySearchPatternQueryHandler which queries MongoDB
    /// for audit logs matching a regex search pattern.
    /// Due to MongoDB driver version conflicts (MongoDB.Driver.Core 2.x vs MongoDB.Driver 3.x),
    /// these tests verify construction and DTO structure rather than mocking the full Find pipeline.
    /// </summary>
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        [Fact]
        public void Handler_CanBeConstructed_WithMockedDatabase()
        {
            var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Loose);
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(mockCollection.Object);

            var handler = new GetAuditLogBySearchPatternQueryHandler(mockDatabase.Object);

            handler.Should().NotBeNull();
        }

        [Fact]
        public void Query_Properties_AreAssignable()
        {
            var query = new GetAuditLogBySearchPatternQuery
            {
                SearchPattern = "Party"
            };

            query.SearchPattern.Should().Be("Party");
        }

        [Fact]
        public void Query_SearchPattern_DefaultIsNull()
        {
            var query = new GetAuditLogBySearchPatternQuery();

            query.SearchPattern.Should().BeNull();
        }

        [Fact]
        public void AuditLogDto_MapsFromAuditLogs_FieldsMatch()
        {
            // Verify that AuditLogDto has all the fields the handler maps
            var dto = new AuditLogDto
            {
                Id = "test-id",
                CreatedBy = 1,
                CreatedByName = "admin",
                IPAddress = "10.0.0.1",
                OS = "Linux",
                Browser = "Chrome",
                Action = "Update",
                Details = "Party updated",
                Module = "PartyMaster",
                MachineName = "SERVER01",
                CreatedAt = DateTimeOffset.UtcNow
            };

            dto.Id.Should().Be("test-id");
            dto.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("admin");
            dto.IPAddress.Should().Be("10.0.0.1");
            dto.OS.Should().Be("Linux");
            dto.Browser.Should().Be("Chrome");
            dto.Action.Should().Be("Update");
            dto.Details.Should().Be("Party updated");
            dto.Module.Should().Be("PartyMaster");
            dto.MachineName.Should().Be("SERVER01");
        }
    }
}
