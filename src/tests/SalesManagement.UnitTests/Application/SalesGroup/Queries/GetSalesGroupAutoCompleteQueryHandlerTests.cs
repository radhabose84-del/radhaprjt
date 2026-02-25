#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesGroup.Queries
{
    public class GetSalesGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesGroupAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<SalesGroupLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<SalesGroupLookupDto> e ? e.ToList() : new List<SalesGroupLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesGroupAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            // Arrange
            var query = new GetSalesGroupAutoCompleteQuery("Group");
            var lookupList = SalesGroupBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Group", It.IsAny<CancellationToken>()))
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
            var query = new GetSalesGroupAutoCompleteQuery("Group");
            var lookupList = SalesGroupBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Group", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result[0].SalesGroupName.Should().Be("Group One");
            result[1].SalesGroupName.Should().Be("Group Two");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            // Arrange
            var query = new GetSalesGroupAutoCompleteQuery("test");
            var lookupList = SalesGroupBuilders.ValidLookupList();

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
            var query = new GetSalesGroupAutoCompleteQuery(null);
            var lookupList = new List<SalesGroupLookupDto>();

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
            var query = new GetSalesGroupAutoCompleteQuery(string.Empty);
            var emptyList = new List<SalesGroupLookupDto>();

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
