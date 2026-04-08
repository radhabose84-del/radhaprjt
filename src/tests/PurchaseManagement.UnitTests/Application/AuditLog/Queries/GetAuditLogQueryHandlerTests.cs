using MongoDB.Driver;
using PurchaseManagement.Application.AuditLog.Queries.GetAuditLog;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogQueryHandlerTests
    {
        private readonly Mock<IMongoDatabase> _mockDb = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<AuditLogs>> _mockCollection = new(MockBehavior.Loose);

        private GetAuditLogQueryHandler CreateSut()
        {
            _mockDb
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);
            return new GetAuditLogQueryHandler(_mockDb.Object);
        }

        [Fact]
        public void Constructor_WithValidDatabase_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_HasNoProperties()
        {
            var query = new GetAuditLogQuery();
            query.Should().NotBeNull();
        }
    }
}
