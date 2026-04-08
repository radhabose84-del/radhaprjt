using Contracts.Common;
using FAM.Application.AuditLog.Queries;
using FAM.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// GetAuditLogBySearchPatternQueryHandler uses IMongoDatabase directly (not a repository interface).
    /// Due to MongoDB.Driver version conflicts (v2 IAsyncCursor vs v3), direct handler mocking
    /// is not feasible. Controller-level tests in AuditLogControllerTests cover the HTTP layer.
    /// These tests verify the query types are correctly defined.
    /// </summary>
    public sealed class GetAuditLogBySearchPatternQueryHandlerTests
    {
        [Fact]
        public void GetAuditLogBySearchPatternQuery_ShouldImplementIRequest()
        {
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = "test" };
            query.Should().BeAssignableTo<IRequest<ApiResponseDTO<List<AuditLogDto>>>>();
        }

        [Fact]
        public void GetAuditLogBySearchPatternQuery_SearchPattern_ShouldBeAssignable()
        {
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = "Create" };
            query.SearchPattern.Should().Be("Create");
        }

        [Fact]
        public void GetAuditLogBySearchPatternQuery_SearchPattern_ShouldAcceptNull()
        {
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = null };
            query.SearchPattern.Should().BeNull();
        }
    }
}
