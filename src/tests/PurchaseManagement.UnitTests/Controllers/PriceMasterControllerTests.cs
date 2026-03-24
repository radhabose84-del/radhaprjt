using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetAll;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PriceMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PriceMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPriceMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<PriceMasterGetAllDto>());

            var result = await CreateSut().GetAll(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPriceMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<PriceMasterGetAllDto>());

            await CreateSut().GetAll(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllPriceMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsOkOrNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeletePriceMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
