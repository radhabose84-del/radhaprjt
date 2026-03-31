using InventoryManagement.Application.Budget.Queries.GetAllBudgets;
using InventoryManagement.Application.Common.Interfaces.Budget;

namespace InventoryManagement.UnitTests.Application.Budget.Queries
{
    public sealed class GetAllBudgetsQueryHandlerTests
    {
        private readonly Mock<IBudgetQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAllBudgetsQueryHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsBudgetList()
        {
            var budgets = new List<BudgetListDto>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };
            _mockRepo.Setup(r => r.GetAllBudgetsAsync(2025)).ReturnsAsync(budgets);

            var result = await CreateSut().Handle(
                new GetAllBudgetsQuery { FiscalYear = 2025 }, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllBudgetsAsync(null)).ReturnsAsync(new List<BudgetListDto>());

            var result = await CreateSut().Handle(
                new GetAllBudgetsQuery { FiscalYear = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
