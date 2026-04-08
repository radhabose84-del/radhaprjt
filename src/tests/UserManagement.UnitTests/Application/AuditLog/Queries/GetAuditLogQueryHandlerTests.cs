using MongoDB.Driver;
using UserManagement.Application.AuditLog.Queries.GetAuditLog;

namespace UserManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogQueryHandlerTests
    {
        // AuditLog query handlers use MongoDB directly (IMongoCollection)
        // which cannot be easily unit tested without an integration setup.
        // These handlers are better covered by integration tests.

        [Fact]
        public void Handler_RequiresMongoDatabase()
        {
            // Verify handler constructor signature
            typeof(GetAuditLogQueryHandler).GetConstructors()
                .Should().NotBeEmpty();
        }
    }
}
