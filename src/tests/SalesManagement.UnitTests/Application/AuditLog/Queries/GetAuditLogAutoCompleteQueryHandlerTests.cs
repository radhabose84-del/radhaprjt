using SalesManagement.Application.AuditLog.Queries;
using SalesManagement.Application.AuditLog.Queries.GetAuditLog;

namespace SalesManagement.UnitTests.Application.AuditLog.Queries;

/// <summary>
/// GetAuditLogBySearchPatternQueryHandler depends on IMongoDatabase with conflicting MongoDB driver versions.
/// These tests verify the query/DTO shape without mocking MongoDB internals.
/// </summary>
public sealed class GetAuditLogAutoCompleteQueryHandlerTests
{
    [Fact]
    public void GetAuditLogBySearchPatternQuery_Properties_AreAssignable()
    {
        var query = new GetAuditLogBySearchPatternQuery { SearchPattern = "test" };
        query.SearchPattern.Should().Be("test");
    }

    [Fact]
    public void AuditLogDto_NullableProperties_AcceptNull()
    {
        var dto = new AuditLogDto
        {
            IPAddress = null,
            MachineName = null,
            OS = null,
            Browser = null
        };

        dto.IPAddress.Should().BeNull();
        dto.MachineName.Should().BeNull();
    }
}
