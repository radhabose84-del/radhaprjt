using BudgetManagement.Application.AuditLog.Queries;
using BudgetManagement.Application.AuditLog.Queries.GetAuditLog;

namespace BudgetManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// Tests for GetAuditLogBySearchPatternQuery and its DTO mapping.
    /// The handler uses IMongoCollection directly (not a repository interface),
    /// and MongoDB.Driver version conflicts (2.15 vs 3.1) prevent mocking IAsyncCursor.
    /// These tests verify query properties and DTO instantiation.
    /// </summary>
    public sealed class GetAuditLogBySearchPatternQueryHandlerTests
    {
        [Fact]
        public void Query_SearchPattern_ShouldBeAssignable()
        {
            var query = new GetAuditLogBySearchPatternQuery
            {
                SearchPattern = "Budget"
            };

            query.SearchPattern.Should().Be("Budget");
        }

        [Fact]
        public void Query_SearchPattern_ShouldAcceptNull()
        {
            var query = new GetAuditLogBySearchPatternQuery
            {
                SearchPattern = null
            };

            query.SearchPattern.Should().BeNull();
        }

        [Fact]
        public void Query_DefaultSearchPattern_ShouldBeNull()
        {
            var query = new GetAuditLogBySearchPatternQuery();
            query.SearchPattern.Should().BeNull();
        }

        [Fact]
        public void AuditLogDto_CanMapMultipleRecords()
        {
            var now = DateTimeOffset.UtcNow;
            var dtos = new List<AuditLogDto>
            {
                new AuditLogDto
                {
                    Id = "id1",
                    Action = "Create",
                    Details = "Created budget group",
                    Module = "BudgetGroup",
                    CreatedAt = now,
                    CreatedBy = 1,
                    CreatedByName = "admin"
                },
                new AuditLogDto
                {
                    Id = "id2",
                    Action = "Update",
                    Details = "Updated budget allocation",
                    Module = "BudgetAllocation",
                    CreatedAt = now.AddMinutes(5),
                    CreatedBy = 2,
                    CreatedByName = "editor"
                }
            };

            dtos.Should().HaveCount(2);
            dtos[0].Action.Should().Be("Create");
            dtos[1].Action.Should().Be("Update");
            dtos[0].Module.Should().Be("BudgetGroup");
            dtos[1].Module.Should().Be("BudgetAllocation");
        }

        [Fact]
        public void AuditLogDto_MachineNameAndBrowser_ShouldBeAssignable()
        {
            var dto = new AuditLogDto
            {
                MachineName = "WORKSTATION-01",
                Browser = "Chrome/120.0",
                OS = "Windows 11 Pro",
                IPAddress = "10.0.0.50"
            };

            dto.MachineName.Should().Be("WORKSTATION-01");
            dto.Browser.Should().Be("Chrome/120.0");
            dto.OS.Should().Be("Windows 11 Pro");
            dto.IPAddress.Should().Be("10.0.0.50");
        }
    }
}
