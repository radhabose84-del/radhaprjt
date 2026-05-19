using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Create.Command;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.CombinePO.Commands
{
    public sealed class CreateCombinePOCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPoMethodLookup> _mockLookup = new(MockBehavior.Loose);

        private CreateCombinePOCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockLookup.Object);

        [Fact]
        public void Constructor_NullMediator_ThrowsArgumentNullException()
        {
            var act = () => new CreateCombinePOCommandHandler(null!, _mockLookup.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("mediator");
        }

        [Fact]
        public void Constructor_NullLookup_ThrowsArgumentNullException()
        {
            var act = () => new CreateCombinePOCommandHandler(_mockMediator.Object, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("lookup");
        }

        [Fact]
        public async Task Handle_LocalMethod_NoLocalData_ReturnsFailure()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new CreateCombinePODto { POMethodId = 1, Local = null };

            var result = await CreateSut().Handle(new CreateCombinePOCommand(dto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Local PO details are required");
        }

        [Fact]
        public async Task Handle_LocalMethod_DelegatesToLocalCommand()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 42 });

            var dto = new CreateCombinePODto
            {
                POMethodId = 1,
                Local = new PurchaseManagement.Application.PurchaseOrder.Dtos.Local.PurchaseOrderCreateDto()
            };

            var result = await CreateSut().Handle(new CreateCombinePOCommand(dto), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ImportMethod_NoImportData_ReturnsFailure()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLookup
                .Setup(l => l.IsImportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new CreateCombinePODto { POMethodId = 2, Import = null };

            var result = await CreateSut().Handle(new CreateCombinePOCommand(dto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Import PO details are required");
        }

        [Fact]
        public async Task Handle_UnsupportedMethod_ReturnsFailure()
        {
            _mockLookup
                .Setup(l => l.IsLocalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLookup
                .Setup(l => l.IsImportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dto = new CreateCombinePODto { POMethodId = 99 };

            var result = await CreateSut().Handle(new CreateCombinePOCommand(dto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Unsupported");
        }
    }
}
