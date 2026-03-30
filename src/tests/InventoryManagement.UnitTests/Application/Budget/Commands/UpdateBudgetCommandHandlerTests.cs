using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Application.Common.Interfaces.Budget;

namespace InventoryManagement.UnitTests.Application.Budget.Commands
{
    public sealed class UpdateBudgetCommandHandlerTests
    {
        private readonly Mock<IBudgetCommandRepository> _mockRepo = new(MockBehavior.Strict);

        private UpdateBudgetCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_WithMasterAmount_ReturnsTrue()
        {
            _mockRepo.Setup(r => r.UpdateBudgetMasterAsync(1, 5000m, "MasterUpdation")).ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new UpdateBudgetCommand { BudgetId = 1, YearBudgetAmount = 5000m },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithDetails_ReturnsTrue()
        {
            _mockRepo.Setup(r => r.UpdateBudgetDetailAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new UpdateBudgetCommand
                {
                    BudgetId = 1,
                    BudgetDetails = new List<UpdateBudgetDetailDto>
                    {
                        new() { DetailId = 1, NewAmount = 1000m, Remarks = "Q1 update" }
                    }
                },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoAmountNoDetails_ReturnsTrue()
        {
            var result = await CreateSut().Handle(
                new UpdateBudgetCommand { BudgetId = 1 },
                CancellationToken.None);

            result.Should().BeTrue();
        }
    }
}
