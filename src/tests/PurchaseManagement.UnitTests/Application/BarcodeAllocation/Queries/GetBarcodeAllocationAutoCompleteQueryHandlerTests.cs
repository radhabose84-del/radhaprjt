using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationAutoComplete;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Queries
{
    public sealed class GetBarcodeAllocationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeAllocationAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeAllocationBuilders.ValidLookupList());

            var result = await CreateSut().Handle(new GetBarcodeAllocationAutoCompleteQuery("BBA"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyString()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeAllocationBuilders.ValidLookupList());

            await CreateSut().Handle(new GetBarcodeAllocationAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
