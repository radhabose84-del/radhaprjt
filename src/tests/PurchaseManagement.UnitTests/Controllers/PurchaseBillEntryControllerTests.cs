using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetById;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PurchaseBillEntryControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);

        private PurchaseBillEntryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseBillEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new PurchaseBillEntryHeaderDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPurchaseBillEntryByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PurchaseBillEntryHeaderDto()));

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPurchaseBillEntryQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PurchaseBillEntryListVm()));

            var result = await CreateSut().GetAllAsync(null, null, null, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithMatchingId_ReturnsOkResult()
        {
            var dto = new PurchaseBillEntryHeaderDto { Id = 1 };

            var result = await CreateSut().UpdateAsync(1, dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithMismatchedId_ReturnsBadRequest()
        {
            var dto = new PurchaseBillEntryHeaderDto { Id = 2 };

            var result = await CreateSut().UpdateAsync(1, dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseBillEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new PurchaseBillEntryHeaderDto());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreatePurchaseBillEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
