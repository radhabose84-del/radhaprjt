using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAutocomplete;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetPurchaseOrderAutocompleteQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetPurchaseOrderAutocompleteHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmpty()
        {
            _mockRepo
                .Setup(r => r.GetAutocompleteAsync(
                    It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<AutocompleteDto>());

            var result = await CreateSut().Handle(
                new GetPurchaseOrderAutocompleteQuery("test", null, null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepoWithCorrectParams()
        {
            _mockRepo
                .Setup(r => r.GetAutocompleteAsync("po", 1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<AutocompleteDto>());

            await CreateSut().Handle(
                new GetPurchaseOrderAutocompleteQuery("po", 1, 2), CancellationToken.None);

            _mockRepo.Verify(r => r.GetAutocompleteAsync("po", 1, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
