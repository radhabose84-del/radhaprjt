using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ImportPOControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);

        private ImportPOController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetImportPOByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ImportPOFullVm?>(new ImportPOFullVm()));

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateImportPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new ImportPOCreateDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateImportPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new ImportPOUpdateDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateImportPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new ImportPOCreateDto());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateImportPOCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
