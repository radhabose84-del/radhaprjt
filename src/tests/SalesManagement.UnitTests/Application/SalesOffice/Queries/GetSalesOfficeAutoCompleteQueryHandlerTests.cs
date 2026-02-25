#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOffice.Queries
{
    public class GetSalesOfficeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesOfficeAutoCompleteQueryHandler CreateSut() =>
            new GetSalesOfficeAutoCompleteQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidTerm_ReturnsResults()
        {
            var query = new GetSalesOfficeAutoCompleteQuery("Office");
            var lookupList = SalesOfficeBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Office", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].SalesOfficeName.Should().Be("Office One");
            result[1].SalesOfficeName.Should().Be("Office Two");
        }

        [Fact]
        public async Task Handle_NullTerm_CallsWithEmptyString()
        {
            var query = new GetSalesOfficeAutoCompleteQuery(null);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesOfficeLookupDto>());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var query = new GetSalesOfficeAutoCompleteQuery("NOTFOUND");
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("NOTFOUND", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesOfficeLookupDto>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ValidTerm_CallsAutocompleteAsync_Once()
        {
            var query = new GetSalesOfficeAutoCompleteQuery("test");
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesOfficeBuilders.ValidLookupList());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
