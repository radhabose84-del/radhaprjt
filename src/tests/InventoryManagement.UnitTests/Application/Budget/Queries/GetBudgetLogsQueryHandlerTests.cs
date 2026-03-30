using InventoryManagement.Application.Budget.Queries.GetBudgetLogs;
using InventoryManagement.Application.Common.Interfaces.Budget;

namespace InventoryManagement.UnitTests.Application.Budget.Queries
{
    public sealed class GetBudgetLogsQueryHandlerTests
    {
        private readonly Mock<IBudgetLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetBudgetLogsQueryHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsBudgetLogs()
        {
            var logs = new List<BudgetLogDto>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };
            _mockRepo.Setup(r => r.GetLogsAsync(1, null)).ReturnsAsync(logs);

            var result = await CreateSut().Handle(
                new GetBudgetLogsQuery { BudgetId = 1 }, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetLogsAsync(null, null)).ReturnsAsync(new List<BudgetLogDto>());

            var result = await CreateSut().Handle(
                new GetBudgetLogsQuery { BudgetId = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
