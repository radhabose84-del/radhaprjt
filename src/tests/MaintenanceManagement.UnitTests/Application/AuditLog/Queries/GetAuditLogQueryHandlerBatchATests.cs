using MaintenanceManagement.Application.AuditLog.Queries.GetAuditLog;
using MaintenanceManagement.Domain.Entities;
using MongoDB.Driver;

namespace MaintenanceManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogQueryHandlerBatchATests
    {
        private readonly Mock<IMongoDatabase> _mockDatabase = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<AuditLogs>> _mockCollection = new(MockBehavior.Loose);

        private GetAuditLogQueryHandler CreateSut()
        {
            _mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);

            return new GetAuditLogQueryHandler(_mockDatabase.Object);
        }

        [Fact]
        public void Constructor_WithValidDatabase_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InjectsMongoDatabaseAndCallsGetCollection()
        {
            _mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);

            _ = new GetAuditLogQueryHandler(_mockDatabase.Object);

            _mockDatabase.Verify(
                d => d.GetCollection<AuditLogs>("AuditLogs", null),
                Times.Once);
        }

        [Fact]
        public void Handler_ImplementsIRequestHandler()
        {
            typeof(MediatR.IRequestHandler<GetAuditLogQuery, List<AuditLogDto>>)
                .IsAssignableFrom(typeof(GetAuditLogQueryHandler))
                .Should().BeTrue();
        }
    }
}
