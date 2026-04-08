using ProductionManagement.Application.AuditLog.Queries.GetAuditLog;
using ProductionManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete;
using ProductionManagement.Application.Common.Interfaces.AuditLog;

namespace ProductionManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAuditLogRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAuditLogAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_WithSearchPattern_ReturnsSuccess()
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
                .Setup(r => r.GetByAuditLogNameAsync("Create"))
                .ReturnsAsync(logs);

            var result = await CreateSut().Handle(
                new GetAuditLogAutoCompleteQuery { SearchPattern = "Create" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptySearch_CallsGetAll()
        {
            var logs = new List<ProductionManagement.Domain.Entities.AuditLogs>
            {
                new() { Action = "Test", CreatedBy = 1, CreatedByName = "admin" }
            };

            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(logs);

            var result = await CreateSut().Handle(
                new GetAuditLogAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailure()
        {
            _mockRepo
                .Setup(r => r.GetByAuditLogNameAsync("nothing"))
                .ReturnsAsync(new List<ProductionManagement.Domain.Entities.AuditLogs>());

            var result = await CreateSut().Handle(
                new GetAuditLogAutoCompleteQuery { SearchPattern = "nothing" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
