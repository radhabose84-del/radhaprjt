using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Application.Budget.Queries.GetAllBudgets;
using InventoryManagement.Application.Budget.Queries.GetBudgetById;
using InventoryManagement.Application.Budget.Queries.GetBudgetLogs;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class BudgetControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private BudgetController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetBudgetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BudgetResponseDto());

            var result = await CreateSut().GetBudgetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetBudgetById_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BudgetResponseDto());

            await CreateSut().GetBudgetById(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetBudgetByIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateBudget_WithValidCommand_ReturnsOkResult()
        {
            var command = new CreateBudgetCommand();
            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateBudget(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateBudget_WithNullCommand_ReturnsBadRequest()
        {
            var result = await CreateSut().CreateBudget(null!);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateBudget_WithValidCommand_ReturnsOkResult()
        {
            var command = new UpdateBudgetCommand { BudgetId = 1 };
            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateBudget(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateBudget_WithInvalidCommand_ReturnsBadRequest()
        {
            var command = new UpdateBudgetCommand { BudgetId = 0 };

            var result = await CreateSut().UpdateBudget(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAllBudgets_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllBudgetsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BudgetListDto>());

            var result = await CreateSut().GetAllBudgets(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetBudgetLogs_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetLogsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BudgetLogDto>());

            var result = await CreateSut().GetBudgetLogs(null, null);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
