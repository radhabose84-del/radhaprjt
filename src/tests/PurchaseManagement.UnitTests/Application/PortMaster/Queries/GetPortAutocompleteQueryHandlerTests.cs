using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Port.Queries.GetPortAutocomplete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PortMaster.Queries
{
    public sealed class GetPortAutocompleteQueryHandlerTests
    {
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetPortAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookupList = new List<PortLookupDto> { PortMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("PORT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetPortAutocompleteQuery("PORT"),
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PortLookupDto>());

            var result = await CreateSut().Handle(
                new GetPortAutocompleteQuery(string.Empty),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsyncOnce()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("P", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PortLookupDto>());

            await CreateSut().Handle(new GetPortAutocompleteQuery("P"), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("P", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
