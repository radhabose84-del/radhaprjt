using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries
{
    public class GetItemPriceMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetItemPriceMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<ItemPriceMasterLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<ItemPriceMasterLookupDto> e ? e.ToList() : new List<ItemPriceMasterLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetItemPriceMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsList_WhenMatchesFound()
        {
            var lookupList = ItemPriceMasterBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetItemPriceMasterAutoCompleteQuery("PC"), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoMatchesFound()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("ZZZ", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesManagement.Application.ItemPriceMaster.Dto.ItemPriceMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetItemPriceMasterAutoCompleteQuery("ZZZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ItemPriceMasterBuilders.ValidLookupList());

            await CreateSut().Handle(
                new GetItemPriceMasterAutoCompleteQuery(null!), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ItemPriceMasterBuilders.ValidLookupList());

            await CreateSut().Handle(
                new GetItemPriceMasterAutoCompleteQuery("PC"), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("PC", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
