using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Update;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.CombinePO.Commands
{
    public sealed class UpdateCombinePOCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPoMethodLookup> _mockLookup = new(MockBehavior.Loose);

        private UpdateCombinePOCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockLookup.Object);

        [Fact]
        public async Task Handle_LocalMethod_DelegatesToLocalUpdate()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new UpdateCombinePODto
            {
                POMethodId = 1,
                Local = new PurchaseManagement.Application.PurchaseOrder.Dtos.Local.PurchaseOrderUpdateDto()
            };

            var result = await CreateSut().Handle(new UpdateCombinePOCommand(dto), CancellationToken.None);

            result.Should().BeTrue();
            _mockMediator.Verify(m => m.Send(It.IsAny<UpdatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UnsupportedMethod_ThrowsInvalidOperationException()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLookup
                .Setup(l => l.IsImportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dto = new UpdateCombinePODto { POMethodId = 99 };

            Func<Task> act = () => CreateSut().Handle(new UpdateCombinePOCommand(dto), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Unsupported POMethodId*");
        }

        [Fact]
        public async Task Handle_ImportMethod_DelegatesToImportUpdate()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLookup
                .Setup(l => l.IsImportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateImportPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new UpdateCombinePODto
            {
                POMethodId = 2,
                Import = new PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO.ImportPOUpdateDto()
            };

            var result = await CreateSut().Handle(new UpdateCombinePOCommand(dto), CancellationToken.None);

            result.Should().BeTrue();
        }
    }
}
