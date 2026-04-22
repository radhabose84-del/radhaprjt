using BudgetManagement.Application.AuditLog.Queries.GetAuditLog;

namespace BudgetManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// Tests for GetAuditLogQuery and AuditLogDto.
    /// The handler itself uses IMongoCollection directly (not a repository interface),
    /// and MongoDB.Driver version conflicts (2.15 vs 3.1) prevent mocking IAsyncCursor.
    /// These tests verify DTO mapping correctness and query instantiation.
    /// </summary>
    public sealed class GetAuditLogQueryHandlerTests
    {
        [Fact]
        public void GetAuditLogQuery_ShouldBeInstantiable()
        {
            var query = new GetAuditLogQuery();
            query.Should().NotBeNull();
        }

        [Fact]
        public void AuditLogDto_AllProperties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new AuditLogDto
            {
                Id = "507f1f77bcf86cd799439011",
                MachineName = "SERVER01",
                IPAddress = "192.168.1.1",
                OS = "Linux",
                Browser = "Firefox",
                Action = "Update",
                Details = "Record updated",
                Module = "MiscMaster",
                CreatedAt = now,
                CreatedBy = 5,
                CreatedByName = "editor"
            };

            dto.Id.Should().Be("507f1f77bcf86cd799439011");
            dto.MachineName.Should().Be("SERVER01");
            dto.IPAddress.Should().Be("192.168.1.1");
            dto.OS.Should().Be("Linux");
            dto.Browser.Should().Be("Firefox");
            dto.Action.Should().Be("Update");
            dto.Details.Should().Be("Record updated");
            dto.Module.Should().Be("MiscMaster");
            dto.CreatedAt.Should().Be(now);
            dto.CreatedBy.Should().Be(5);
            dto.CreatedByName.Should().Be("editor");
        }

        [Fact]
        public void AuditLogDto_NullableProperties_ShouldAcceptNull()
        {
            var dto = new AuditLogDto
            {
                Id = null,
                MachineName = null,
                IPAddress = null,
                OS = null,
                Browser = null,
                Action = null,
                Details = null,
                Module = null,
                CreatedByName = null
            };

            dto.Id.Should().BeNull();
            dto.MachineName.Should().BeNull();
            dto.IPAddress.Should().BeNull();
            dto.OS.Should().BeNull();
            dto.Browser.Should().BeNull();
            dto.Action.Should().BeNull();
            dto.Details.Should().BeNull();
            dto.Module.Should().BeNull();
            dto.CreatedByName.Should().BeNull();
        }

        [Fact]
        public void AuditLogDto_DefaultCreatedBy_ShouldBeZero()
        {
            var dto = new AuditLogDto();
            dto.CreatedBy.Should().Be(0);
        }

        [Fact]
        public void AuditLogDto_DefaultCreatedAt_ShouldBeDefault()
        {
            var dto = new AuditLogDto();
            dto.CreatedAt.Should().Be(default(DateTimeOffset));
        }
    }
}
