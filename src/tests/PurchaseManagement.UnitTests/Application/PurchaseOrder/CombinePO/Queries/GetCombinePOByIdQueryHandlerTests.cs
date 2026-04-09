using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.CombinePO.Queries
{
    public sealed class GetCombinePOByIdQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPoMethodLookup> _mockLookup = new(MockBehavior.Loose);

        private GetCombinePOByIdQueryHandler CreateSut() =>
            new(_mockMediator.Object, _mockLookup.Object);

        [Fact]
        public async Task Handle_LocalPO_ReturnsLocalVm()
        {
            _mockLookup.Setup(l => l.IsLocalAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPurchaseOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PurchaseOrderDetailDto());

            var result = await CreateSut().Handle(
                new GetCombinePOByIdQuery(10, 1), CancellationToken.None);

            result.Local.Should().NotBeNull();
            result.Import.Should().BeNull();
        }

        [Fact]
        public async Task Handle_InvalidMethod_ThrowsInvalidOperation()
        {
            _mockLookup.Setup(l => l.IsLocalAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockLookup.Setup(l => l.IsImportAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetCombinePOByIdQuery(10, 99), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
