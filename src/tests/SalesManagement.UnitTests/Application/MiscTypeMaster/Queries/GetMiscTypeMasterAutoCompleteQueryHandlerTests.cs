#nullable disable
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;
using SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new GetMiscTypeMasterAutoCompleteQueryHandler(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_WithMatchingTerm_ReturnsLookupList()
        {
            var lookupList = MiscTypeMasterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("MISC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("MISC"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectLookupData()
        {
            var lookupList = MiscTypeMasterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("MISC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("MISC"), CancellationToken.None);

            result[0].MiscTypeCode.Should().Be("MISC001");
            result[1].MiscTypeCode.Should().Be("MISC002");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscTypeMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsync_Once_WithCorrectTerm()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("MISC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscTypeMasterLookupDto>());

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("MISC"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("MISC", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("ZZZZ", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscTypeMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("ZZZZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
