using InventoryManagement.Application.Budget.Queries.GetBudgetById;
using InventoryManagement.Application.Common.Interfaces.Budget;

namespace InventoryManagement.UnitTests.Application.Budget.Queries
{
    public sealed class GetBudgetByIdQueryHandlerTests
    {
        private readonly Mock<IBudgetQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetBudgetByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsBudget()
        {
            var dto = new BudgetResponseDto { Id = 1 };
            _mockRepo.Setup(r => r.GetBudgetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetBudgetByIdQuery { BudgetId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetBudgetByIdAsync(99)).ReturnsAsync((BudgetResponseDto?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetBudgetByIdQuery { BudgetId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*99*");
        }
    }
}
