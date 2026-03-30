using AutoMapper;
using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Application.Features.Budget.Commands.CreateBudget;
using InventoryManagement.Domain.Entities.Budget;

namespace InventoryManagement.UnitTests.Application.Budget.Commands
{
    public sealed class CreateBudgetCommandHandlerTests
    {
        private readonly Mock<IBudgetCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateBudgetCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            _mockMapper.Setup(m => m.Map<BudgetMaster>(It.IsAny<CreateBudgetCommand>()))
                .Returns(new BudgetMaster());
            _mockRepo.Setup(r => r.CreateBudgetAsync(It.IsAny<BudgetMaster>())).ReturnsAsync(10);

            var result = await CreateSut().Handle(new CreateBudgetCommand(), CancellationToken.None);

            result.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            _mockMapper.Setup(m => m.Map<BudgetMaster>(It.IsAny<CreateBudgetCommand>()))
                .Returns(new BudgetMaster());
            _mockRepo.Setup(r => r.CreateBudgetAsync(It.IsAny<BudgetMaster>())).ReturnsAsync(1);

            await CreateSut().Handle(new CreateBudgetCommand(), CancellationToken.None);

            _mockRepo.Verify(r => r.CreateBudgetAsync(It.IsAny<BudgetMaster>()), Times.Once);
        }
    }
}
