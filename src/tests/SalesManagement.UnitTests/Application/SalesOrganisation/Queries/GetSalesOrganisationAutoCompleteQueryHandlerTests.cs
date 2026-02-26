using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Queries
{
    public class GetSalesOrganisationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesOrganisationAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<SalesOrganisationLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<SalesOrganisationLookupDto> e ? e.ToList() : new List<SalesOrganisationLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesOrganisationAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            // Arrange
            var query = new GetSalesOrganisationAutoCompleteQuery("ORG");
            var lookupList = SalesOrganisationBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("ORG", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsCorrectLookupData()
        {
            // Arrange
            var query = new GetSalesOrganisationAutoCompleteQuery("ORG");
            var lookupList = SalesOrganisationBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("ORG", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result[0].SalesOrganisationCode.Should().Be("ORG001");
            result[1].SalesOrganisationCode.Should().Be("ORG002");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            // Arrange
            var query = new GetSalesOrganisationAutoCompleteQuery("test");
            var lookupList = SalesOrganisationBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            // Act
            await sut.Handle(query, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            // Arrange — handler uses r.Term ?? string.Empty
            var query = new GetSalesOrganisationAutoCompleteQuery(null!);
            var lookupList = new List<SalesOrganisationLookupDto>();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert — repository must be called with empty string, not null
            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetSalesOrganisationAutoCompleteQuery(string.Empty);
            var emptyList = new List<SalesOrganisationLookupDto>();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
