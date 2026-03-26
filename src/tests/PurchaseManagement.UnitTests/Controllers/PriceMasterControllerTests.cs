using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PriceMaster.Commands.Create;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Application.PriceMaster.Commands.Update;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetAll;
using PurchaseManagement.Application.PriceMaster.Queries.GetById;
using PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
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

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(new PriceMasterCreateDto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().Create(new PriceMasterCreateDto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreatePriceMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WhenSuccess()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new PriceMasterUpdateDto { Id = 1 }, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenFailed()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePriceMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Update(new PriceMasterUpdateDto { Id = 99 }, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPriceMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PriceMasterGetAllDto { Id = 1 });

            var result = await CreateSut().GetById(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPriceMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);

            var result = await CreateSut().GetById(99, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPriceMasterPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<PriceMasterPendingGroupDto>(), 0));

            var result = await CreateSut().GetPendingAsync(ct: CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
