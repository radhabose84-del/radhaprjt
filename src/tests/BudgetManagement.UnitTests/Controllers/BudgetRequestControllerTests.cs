using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.BudgetRequest.Commands.Delete;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.BudgetRequest.Queries.GetAll;
using BudgetManagement.Application.BudgetRequest.Queries.GetById;
using BudgetManagement.Presentation.Controllers;
using BudgetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.UnitTests.Controllers
{
    public sealed class BudgetRequestControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private BudgetRequestController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllBudgetRequestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetRequestListItemDto>(), 0));

            var result = await CreateSut().GetAll();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllBudgetRequestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetRequestListItemDto>(), 0));

            await CreateSut().GetAll();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllBudgetRequestQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetRequestBuilders.ValidDto(1));

            var result = await CreateSut().GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_NullResult_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetRequestDto)null!);

            var result = await CreateSut().GetById(99);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBudgetRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(BudgetRequestBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateBudgetRequestCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Update(BudgetRequestBuilders.ValidUpdateCommand(id: 1));

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ZeroId_ReturnsBadRequest()
        {
            var result = await CreateSut().Update(BudgetRequestBuilders.ValidUpdateCommand(id: 0));

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBudgetRequestCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
