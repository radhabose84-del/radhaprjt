using Contracts.Common;
using MongoDB.Driver;
using PurchaseManagement.Application.AuditLog.Queries;
using PurchaseManagement.Application.AuditLog.Queries.GetAuditLog;
using PurchaseManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMongoDatabase> _mockDb = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<AuditLogs>> _mockCollection = new(MockBehavior.Loose);

        private GetAuditLogBySearchPatternQueryHandler CreateSut()
        {
            _mockDb
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);
            return new GetAuditLogBySearchPatternQueryHandler(_mockDb.Object);
        }

        [Fact]
        public void Constructor_WithValidDatabase_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = "test" };
            query.SearchPattern.Should().Be("test");
        }
    }
}
