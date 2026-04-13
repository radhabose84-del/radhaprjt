using Contracts.Common;
using MaintenanceManagement.Application.AuditLog.Queries;
using MaintenanceManagement.Application.AuditLog.Queries.GetAuditLog;
using MaintenanceManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;
using MaintenanceManagement.Domain.Entities;
using MongoDB.Driver;

namespace MaintenanceManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerBatchATests
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
        public void Constructor_InjectsMongoDatabaseAndCallsGetCollection()
        {
            _mockDatabase
                .Setup(d => d.GetCollection<AuditLogs>("AuditLogs", null))
                .Returns(_mockCollection.Object);

            _ = new GetAuditLogBySearchPatternQueryHandler(_mockDatabase.Object);

            _mockDatabase.Verify(
                d => d.GetCollection<AuditLogs>("AuditLogs", null),
                Times.Once);
        }

        [Fact]
        public void Handler_ImplementsIRequestHandler()
        {
            typeof(MediatR.IRequestHandler<GetAuditLogBySearchPatternQuery, ApiResponseDTO<List<AuditLogDto>>>)
                .IsAssignableFrom(typeof(GetAuditLogBySearchPatternQueryHandler))
                .Should().BeTrue();
        }
    }
}
