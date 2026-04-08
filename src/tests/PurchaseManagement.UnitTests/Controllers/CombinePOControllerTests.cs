using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Command;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class CombinePOControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CombinePOController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().Create(new CreateCombinePODto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            await CreateSut().Create(new CreateCombinePODto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateCombinePOCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Amend_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AmendCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var result = await CreateSut().Amend(new AmendCombinePODto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Amend_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AmendCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            await CreateSut().Amend(new AmendCombinePODto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<AmendCombinePOCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCombinePOByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCombinePOByIdVm());

            var result = await CreateSut().GetById(1, 1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCombinePOByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCombinePOByIdVm());

            await CreateSut().GetById(1, 1, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCombinePOByIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateCombinePODto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCombinePOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Update(new UpdateCombinePODto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UpdateCombinePOCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
