using GateEntryManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;
using GateEntryManagement.Domain.Entities;
using MongoDB.Driver;

namespace GateEntryManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMongoDatabase> _mockDatabase = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<AuditLogs>> _mockCollection = new(MockBehavior.Loose);

        private GetAuditLogBySearchPatternQueryHandler CreateSut()
        {
            _mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);

            return new GetAuditLogBySearchPatternQueryHandler(_mockDatabase.Object);
        }

        [Fact]
        public void Constructor_WithValidDatabase_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InjectsMongoDatabase()
        {
            _mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);

            var sut = new GetAuditLogBySearchPatternQueryHandler(_mockDatabase.Object);

            _mockDatabase.Verify(
                d => d.GetCollection<AuditLogs>("AuditLogs", null),
                Times.Once);
        }
    }
}
