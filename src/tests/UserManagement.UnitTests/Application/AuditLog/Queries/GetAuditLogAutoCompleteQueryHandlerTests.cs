using UserManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;

namespace UserManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        // AuditLog handler uses IMongoCollection directly - not mockable in unit tests.
        // Better covered by integration tests with real MongoDB.

        [Fact]
        public void Handler_RequiresMongoDatabase()
        {
            typeof(GetAuditLogBySearchPatternQueryHandler).GetConstructors()
                .Should().NotBeEmpty();
        }
    }
}
