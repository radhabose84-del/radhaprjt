using ProductionManagement.Application.AuditLog.Queries.GetAuditLog;
using ProductionManagement.Application.Common.Interfaces.AuditLog;

namespace ProductionManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogQueryHandlerTests
    {
        private readonly Mock<IAuditLogRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAuditLogQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_WithLogs_ReturnsSuccess()
        {
            var logs = new List<ProductionManagement.Domain.Entities.AuditLogs>
            {
                new()
                {
                    Action = "Create",
                    Details = "Test",
                    Module = "Production",
                    CreatedBy = 1,
                    CreatedByName = "admin"
                }
            };

            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(logs);

            var result = await CreateSut().Handle(new GetAuditLogQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoLogs_ReturnsFailure()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProductionManagement.Domain.Entities.AuditLogs>());

            var result = await CreateSut().Handle(new GetAuditLogQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
