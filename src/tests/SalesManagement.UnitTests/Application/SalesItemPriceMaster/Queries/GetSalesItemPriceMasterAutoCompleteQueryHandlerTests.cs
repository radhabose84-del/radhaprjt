#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Queries
{
    public class GetSalesItemPriceMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesItemPriceMasterAutoCompleteQueryHandler CreateSut() =>
            new GetSalesItemPriceMasterAutoCompleteQueryHandler(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsList_WhenMatchesFound()
        {
            var lookupList = SalesItemPriceMasterBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterAutoCompleteQuery("PC"), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoMatchesFound()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("ZZZ", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterAutoCompleteQuery("ZZZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesItemPriceMasterBuilders.ValidLookupList());

            await CreateSut().Handle(
                new GetSalesItemPriceMasterAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesItemPriceMasterBuilders.ValidLookupList());

            await CreateSut().Handle(
                new GetSalesItemPriceMasterAutoCompleteQuery("PC"), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
