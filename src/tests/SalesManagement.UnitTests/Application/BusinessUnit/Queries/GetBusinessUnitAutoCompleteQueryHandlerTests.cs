using AutoMapper;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitAutoComplete;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Queries
{
    /// <summary>
    /// ⚠️ BusinessUnit AutoComplete passes Term directly — no null→"" conversion.
    /// When Term is null, the repository is called with null (unlike SalesChannel which uses ?? string.Empty).
    /// </summary>
    public class GetBusinessUnitAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetBusinessUnitAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<BusinessUnitLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<BusinessUnitLookupDto> e ? e.ToList() : new List<BusinessUnitLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetBusinessUnitAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var query = new GetBusinessUnitAutoCompleteQuery("BU");
            var lookupList = BusinessUnitBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("BU", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsCorrectLookupData()
        {
            var query = new GetBusinessUnitAutoCompleteQuery("BU");
            var lookupList = BusinessUnitBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("BU", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result[0].BusinessUnitCode.Should().Be("BU001");
            result[1].BusinessUnitCode.Should().Be("BU002");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            var query = new GetBusinessUnitAutoCompleteQuery("test");
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(BusinessUnitBuilders.ValidLookupList());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesNullToRepository()
        {
            // BusinessUnit handler passes Term directly — null stays null (unlike SalesChannel)
            var query = new GetBusinessUnitAutoCompleteQuery(null!);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BusinessUnitLookupDto>());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(null!, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            var query = new GetBusinessUnitAutoCompleteQuery(string.Empty);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BusinessUnitLookupDto>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
