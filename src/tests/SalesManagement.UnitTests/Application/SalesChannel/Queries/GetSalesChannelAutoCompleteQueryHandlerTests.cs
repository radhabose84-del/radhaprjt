#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Application.SalesChannel.Queries.GetSalesChannelAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesChannel.Queries
{
    public class GetSalesChannelAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesChannelAutoCompleteQueryHandler CreateSut() =>
            new GetSalesChannelAutoCompleteQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var query = new GetSalesChannelAutoCompleteQuery("CH");
            var lookupList = SalesChannelBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("CH", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsCorrectLookupData()
        {
            var query = new GetSalesChannelAutoCompleteQuery("CH");
            var lookupList = SalesChannelBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("CH", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result[0].SalesChannelCode.Should().Be("CH001");
            result[1].SalesChannelCode.Should().Be("CH002");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            var query = new GetSalesChannelAutoCompleteQuery("test");
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesChannelBuilders.ValidLookupList());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            // Handler uses r.Term ?? string.Empty
            var query = new GetSalesChannelAutoCompleteQuery(null);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesChannelLookupDto>());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            var query = new GetSalesChannelAutoCompleteQuery(string.Empty);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesChannelLookupDto>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
