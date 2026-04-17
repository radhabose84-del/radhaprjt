using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.AmendCombinePO;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.CombinePO.Commands
{
    public sealed class AmendCombinePOCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPoMethodLookup> _mockLookup = new(MockBehavior.Loose);

        private AmendCombinePOCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockLookup.Object);

        [Fact]
        public async Task Handle_LocalMethod_DelegatesToPOAmendment()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<POAmendmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var dto = new AmendCombinePODto
            {
                POMethodId = 1,
                Local = new PurchaseManagement.Application.PurchaseOrder.Dtos.Local.PurchaseOrderUpdateDto()
            };

            var result = await CreateSut().Handle(new AmendCombinePOCommand(dto), CancellationToken.None);

            result.Should().Be(42);
            _mockMediator.Verify(m => m.Send(It.IsAny<POAmendmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ImportMethod_DelegatesToImportPOAmendment()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLookup
                .Setup(l => l.IsImportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ImportPOAmendmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            var dto = new AmendCombinePODto
            {
                POMethodId = 2,
                Import = new PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO.ImportPOUpdateDto()
            };

            var result = await CreateSut().Handle(new AmendCombinePOCommand(dto), CancellationToken.None);

            result.Should().Be(10);
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

            var dto = new AmendCombinePODto { POMethodId = 99 };

            Func<Task> act = () => CreateSut().Handle(new AmendCombinePOCommand(dto), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Unsupported POMethodId*");
        }
    }
}
