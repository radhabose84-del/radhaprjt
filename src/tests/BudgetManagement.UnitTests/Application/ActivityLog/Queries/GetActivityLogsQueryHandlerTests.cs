using BudgetManagement.Domain.Entities;

namespace BudgetManagement.UnitTests.Application.ActivityLog.Queries
{
    public sealed class GetActivityLogsQueryHandlerTests
    {
        private readonly Mock<IActivityLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetActivityLogsQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var data = new List<BudgetManagement.Domain.Entities.ActivityLog> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllAsync("Entity", 1, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync((data, 1));

            var result = await CreateSut().Handle(new GetActivityLogsQuery("Entity", 1, 1, 10), CancellationToken.None);
            result.Item1.Should().HaveCount(1);
        }
    }
}
